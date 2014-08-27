using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewerIntegration
{
    static class UriSchemeValidator
    {
        // TODO adapt regex pattern, so that servername without servergroup at the end is accepted as well.
        private const string action                 = @"&action=(?<action>query|retrieve|open)";
        private const string level                  = @"&level=(?<level>study|series)";
        private const string queryParam             = @"&queryparam=(?<dicomtag>.*)=(?<dicomvalue>.*)";
        private const string server                 = @"(&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*))";

        //private const string strRegex = @"&action=(?<action>query|retrieve|open)&level=(?<level>study|series)&query=(?<dicomtag>[0-9a-fA-F]{4}:[0-9a-fA-F]{4})=(?<dicomvalue>\d+(?:\.\d+)*)(&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*))";

        // BE CAREFULL!!!! regex without & at the beginning of action still gives a match!!!, due to conditional server part in the end!!
        // http://regexhero.net/tester/

        // works only isolated!!! but not properly with all the others together
        //@"&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*)";
        
        // (&servername=.*&servergroup=.* | &servername=(?<servername>.*)&servergroup=(?<servergroup>.*) | &servername=(?<servername>.*))

        private static Regex urlValidationRegex;

        public static string ActionPattern()
        { 
            return action;
        }

        public static string LevelPattern()
        {
            return level;
        }

        public static string QueryParamPattern()
        {
            return queryParam;
        }

        public static string ServerPattern()
        {
            return server;
        }

        public static string TotalPattern()
        {
            //return strRegex;
            return action + level + queryParam + server;
        }

        // see C:\Users\UIB\git\clearcanvas\Dicom\Validation\DicomValidator.cs
        private static Regex UrlValidationRegex
        {
            get
            {
                if (urlValidationRegex == null)
                    urlValidationRegex = new Regex(UriSchemeValidator.TotalPattern(), RegexOptions.ExplicitCapture);

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
