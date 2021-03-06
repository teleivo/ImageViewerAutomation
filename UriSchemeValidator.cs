﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewerIntegration
{
    static class UriSchemeValidator
    {
        //private const string action                 = @"&action=(?<action>query|retrieve|open)";
        //private const string queryParam             = @"&queryparam=(?<dicomtag>.*)=(?<dicomvalue>.*)";
        //private const string param                  = @"&parameter=(?<param>.*)";
        //private const string server                 = @"(?:&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*))";

        private const string regexPattern = @"&action=(?<action>.*)&parameter=(?<param>.*)(?:&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*))";
        private static Regex urlValidationRegex;

        // see C:\Users\UIB\git\clearcanvas\Dicom\Validation\DicomValidator.cs
        private static Regex UrlValidationRegex
        {
            get
            {
                if (urlValidationRegex == null)
                    urlValidationRegex = new Regex(regexPattern, RegexOptions.ExplicitCapture);

                return urlValidationRegex;
            }
        }

        // NICE BUT NOT NECESSARILY NEEDED, leave it for now
        /// <summary>
        /// Validate the specified URL conforms to URI scheme defined for this application.
        /// </summary>
        public static void ValidateUrl(string url)
        {
            if (String.IsNullOrEmpty(url) || url.TrimEnd(' ').Length == 0)
                return;// ok

            if (!UrlValidationRegex.IsMatch(url))
            {
                throw new ArgumentException("Malformed URL\n\t" + url);
            }
        }

        /// <summary>
        /// Validate the specified URL conforms to URI scheme defined for this application & parse it into a GroupCollection.
        /// </summary>
        public static GroupCollection ParseUrl(string url)
        {
            if (String.IsNullOrEmpty(url) || url.TrimEnd(' ').Length == 0)
                return null;

            // Match the regular expression pattern against a text string.
            Match matchUrl = UrlValidationRegex.Match(url);

            if (matchUrl.Success)
            {
                return matchUrl.Groups;
            }
            else
            {
                throw new ArgumentException("Malformed URL\n\t{0}", url);
            }
        }

        public static string[] UrlValidationGroupNames()
        {
            if (urlValidationRegex == null)
                return null;
            else
                return urlValidationRegex.GetGroupNames();  
        }

    }
}
