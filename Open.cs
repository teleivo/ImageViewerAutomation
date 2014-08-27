using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewerIntegration.DicomExplorerAutomation;
using System.Text.RegularExpressions;
using System.Reflection;

namespace ViewerIntegration
{
    public class Open : ImageViewerAutomationAction
    {
        private const string regexActionType = "open";
        private const string regexActionParameter = ".*=.*";
        private static UriScheme uriQueryScheme = new UriScheme(regexActionParameter);

        private ServerNode server;
        private DicomExplorerSearchCriteria querySearchCriteria;

        public ServerNode ActionOnServer
        {
            set
            {
                server = value;
            }

            get
            {
                return server;
            }
        }

        public string RegexPatternActionType
        {
            get
            {
                return regexActionType;
            }
        }

        public string RegexPatternActionParameter
        {
            get
            {
                return regexActionParameter;
            }
        }

        /// <summary>
        /// Validate the specified URL conforms to URI scheme defined for this application & parse it into a GroupCollection.
        /// </summary>
        public static Query ParseUrl(string url)
        {
            GroupCollection matchedGroups = uriQueryScheme.ParseUrl(url);

            DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();

            Console.WriteLine("UriScheme.ParseUrl() found\n");
            foreach (string groupName in uriQueryScheme.UrlValidationGroupNames())
            {
                Console.WriteLine(
                   "Group: {0}, Value: {1}",
                   groupName,
                   matchedGroups[groupName].Value);

                // try if parameters are contained in DicomExplorerSearchCriteria
                try
                {
                    PropertyInfo studyCriteriaProperty = studyCriteria.GetType().GetProperty(groupName);
                    studyCriteriaProperty.SetValue(studyCriteria, matchedGroups[groupName].Value, null);
                }
                catch (Exception e)
                {
                    throw new ArgumentOutOfRangeException("DicomExplorerSearchCriteria does not contain property '{0}'", groupName);
                }
            }

            // TODO reaction if studyCriteria is null!

            return new Query() { querySearchCriteria = studyCriteria };
        }

        public Boolean Execute()
        {
            if (ActionOnServer.isLocal)
                return ExecuteQueryLocal();
            else
                return ExecuteQueryRemote();
        }

        public override Boolean ExecuteQueryRemote()
        {
            //TODO what to do when request comes in with a Study Instance UID? Get PatientId and maybe Accession Nr 

            DicomExplorerAutomationClient dicomExplorerAutomationClient = new DicomExplorerAutomationClient("DicomExplorerAutomation");

            //SearchRemoteStudiesRequest studySearchRemote = new SearchRemoteStudiesRequest();
            ////DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();
            //DicomExplorerSearchCriteria studyCriteria = actionParameter.ToDicomExplorerSearchCriteria();
            //studySearchRemote.AETitle = executeActionOnServer.aeTitle;
            //studySearchRemote.SearchCriteria = studyCriteria;

            //SearchLocalStudiesRequest studySearchLocal = new SearchLocalStudiesRequest();
            DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();
            //studyCriteria.PatientId = GetPatientId("PatientId", "20140121.122619");
            studyCriteria.PatientId = "20140121.122619";

            // ADD INTO THIS LATER: SearchCriteria = querySearchCriteria
            SearchRemoteStudiesRequest studySearchRemote = new SearchRemoteStudiesRequest()
            {
                SearchCriteria = studyCriteria,
                AETitle = ActionOnServer.aeTitle
            };
            SearchRemoteStudiesResult resultSearchRemote;

            try
            {
                resultSearchRemote = dicomExplorerAutomationClient.SearchRemoteStudies(studySearchRemote);
                dicomExplorerAutomationClient.Close();
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }

        public Boolean ExecuteQueryLocal()
        {
            //TODO what to do when request comes in with a Study Instance UID? Get PatientId and maybe Accession Nr 

            DicomExplorerAutomationClient dicomExplorerAutomationClient = new DicomExplorerAutomationClient("DicomExplorerAutomation");

            //SearchLocalStudiesRequest studySearchLocal = new SearchLocalStudiesRequest();
            DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();
            //studyCriteria.PatientId = GetPatientId("PatientId", "20140121.122619");
            studyCriteria.PatientId = "20140121.122619";

            //studySearchLocal.SearchCriteria = studyCriteria;

            // ADD INTO THIS LATER: SearchCriteria = querySearchCriteria
            SearchLocalStudiesRequest studySearchLocal = new SearchLocalStudiesRequest()
            {
                SearchCriteria = studyCriteria
            };

            SearchLocalStudiesResult resultSearchLocal;

            try
            {
                resultSearchLocal = dicomExplorerAutomationClient.SearchLocalStudies(studySearchLocal);
                dicomExplorerAutomationClient.Close();
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }
    }
}
