using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewerIntegration
{
    public class UriScheme
    {
        private Regex urlValidationRegex;

        // see C:\Users\UIB\git\clearcanvas\Dicom\Validation\DicomValidator.cs
        public UriScheme(string regexPattern)
        {
            urlValidationRegex = new Regex(regexPattern, RegexOptions.ExplicitCapture);
        }

        // NICE BUT NOT NECESSARILY NEEDED, leave it for now
        /// <summary>
        /// Validate the specified URL conforms to URI scheme defined for this application.
        /// </summary>
        public void ValidateUrl(string url)
        {
            if (String.IsNullOrEmpty(url) || url.TrimEnd(' ').Length == 0)
                return;// ok

            if (!urlValidationRegex.IsMatch(url))
            {
                throw new ArgumentException("Malformed URL\n\t" + url);
            }
        }

        /// <summary>
        /// Validate the specified URL conforms to URI scheme defined for this application & parse it into a GroupCollection.
        /// </summary>
        public GroupCollection ParseUrl(string url)
        {
            if (String.IsNullOrEmpty(url) || url.TrimEnd(' ').Length == 0)
                return null;

            // Match the regular expression pattern against a text string.
            Match matchUrl = urlValidationRegex.Match(url);

            if (matchUrl.Success)
            {
                return matchUrl.Groups;
            }
            else
            {
                throw new ArgumentException("Malformed URL\n\t{0}", url);
            }
        }

        public string[] UrlValidationGroupNames()
        {
            if (urlValidationRegex == null)
                return null;
            else
                return urlValidationRegex.GetGroupNames();
        }

    }
}
