using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using ViewerIntegration.ViewerAutomation;

namespace ViewerIntegration
{
    public class ImageViewerAutomation
    {
        public class Pacs
        { 
            private readonly string _aeTitle;
            private readonly string _ipAddress;
            private readonly string _port;

            public Pacs(string aeTitle, string ipAddress, string port)
            {
                _aeTitle = aeTitle;
                _ipAddress = ipAddress;
                _port = port;

            }

            public string AeTitle 
            {  
                get
                {
                    return _aeTitle;
                }
            }

            public string IpAddress
            {
                get
                {
                    return _ipAddress;
                }
            }

            public string Port
            {
                get
                {
                    return _port;
                }
            }

        }

        public class Query
        {
            private readonly string _level;
            private readonly string _dicomTag;
            private readonly string _dicomValue;

            public Query(string level, string dicomTag, string dicomValue)
            {
                _level = level;
                _dicomTag = dicomTag;
                _dicomValue = dicomValue;

            }

            public string Level
            {
                get
                {
                    return _level;
                }
            }

            public string DicomTag
            {
                get
                {
                    return _dicomTag;
                }
            }

            public string DicomValue
            {
                get
                {
                    return _dicomValue;
                }
            }

        }

        public class ViewerAction
        {
            //TODO call parsing URL method in the constructor or do it from elsewhere?
            //TODO error check in URL

            // Field 
            private readonly string _url;
            private string _action;
            public Query query;
            public Pacs pacs;

            // Constructor that takes one arguments. 
            public ViewerAction(string argUrl)
            {
                _url = argUrl;
                ParseUrl(argUrl);
            }

            public string Url
            {
                get
                {
                    return _url;
                }
            }

            public string Action
            {
                get
                {
                    return _action;
                }
            }

            private Boolean ParseUrl(string argUrl)
            {

                Console.WriteLine(UrlPattern.TotalPattern());
                
                // Instantiate the regular expression object.
                Regex r = new Regex(UrlPattern.TotalPattern(), RegexOptions.ExplicitCapture);

                // Match the regular expression pattern against a text string.
                Match m = r.Match(argUrl);

                if (m.Success)
                {

                    for (int ctr = 1; ctr < m.Groups.Count; ctr++)
                        Console.WriteLine("   Group {0}: {1}", ctr, m.Groups[ctr].Value);

                    _action = m.Groups["action"].Value;
                    query = new Query(m.Groups["level"].Value, m.Groups["dicomtag"].Value, m.Groups["dicomvalue"].Value);
                    pacs = new Pacs(m.Groups["pacsaetitle"].Value, m.Groups["pacsip"].Value, m.Groups["pacsport"].Value);

                    return true;
                }
                return false;

            }

            public Boolean ExecuteAction()
            {
                // Connect to the ClearCanvas ImageViewer Automation service
                ViewerAutomationClient client = new ViewerAutomationClient("ViewerAutomation");
                // Launch the ClearCanvas ImageViewer if not already open
                StartViewer();

                // OPENS LOCAL/ImageServer STUDY IN VIEWER!
                OpenStudyInfo info = new OpenStudyInfo();
                info.StudyInstanceUid = query.DicomValue;
                OpenStudyInfo[] studiesToOpen = new OpenStudyInfo[] { info };
                OpenStudiesRequest request = new OpenStudiesRequest();
                request.StudiesToOpen = studiesToOpen;

                request.ActivateIfAlreadyOpen = true;

                //SearchRemoteStudiesRequest studySearch = new SearchRemoteStudiesRequest();
                //DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();
                //studyCriteria.PatientId = "20140121.124070";
                //studySearch.AETitle = viewerAction.pacs.AeTitle;
                //studySearch.SearchCriteria = studyCriteria;
                //SearchRemoteStudiesResult remoteresult;

                try
                {
                    client.OpenStudies(request);

                    //remoteresult = client1.SearchRemoteStudies(studySearch);
                    client.Close();

                }
                catch (Exception e)
                {
                    string message = string.Format("{0} \n\n", e.Message);

                    Console.WriteLine(message);

                }
                
                return true;

            }

            public static void StartViewer()
            {
                const string processName = "ClearCanvas.Desktop.Executable";
                const string viewerProcessPath = @"C:\Program Files\ClearCanvas\ClearCanvas Workstation";

                Process[] viewerProcesses = Process.GetProcessesByName(processName);
                if (viewerProcesses == null || viewerProcesses.Length == 0)
                {
                    string executable = Path.Combine(viewerProcessPath, processName) + ".exe";

                    ProcessStartInfo startInfo = new ProcessStartInfo(executable, "");
                    startInfo.WorkingDirectory = viewerProcessPath;
                    Process viewerProcess = Process.Start(startInfo);
                    if (viewerProcess == null)
                        Console.WriteLine("Failed to start the viewer process.");
                }
            }  

        }

    }
}

//studyInstanceUid = args[0].Replace("ccautomation:", "");
//Console.WriteLine(studyInstanceUid);
//Console.WriteLine(args[0]);
//String studyInstanceUid;
//Console.WriteLine(studyInstanceUid);

//StartViewer();

//try
//{
//    ViewerAutomationClient client = new ViewerAutomationClient("ViewerAutomation");

//    // Use the 'client' variable to call operations on the service.
//    // Always close the client.
//    //client.Close();

//    // OPENS LOCAL STUDY or STUDY FROM DEFAULT PACS (which has to be a CLEARCANVAS ImageStreaming PACS) IN VIEWER!
//    OpenStudyInfo info = new OpenStudyInfo();
//    info.StudyInstanceUid = studyInstanceUid;
//    OpenStudyInfo[] studiesToOpen = new OpenStudyInfo[] { info };
//    OpenStudiesRequest request = new OpenStudiesRequest();
//    request.StudiesToOpen = studiesToOpen;

//    request.ActivateIfAlreadyOpen = true;

//    client.OpenStudies(request);
//    client.Close();
//}
//catch (Exception e)
//{
//    string message = string.Format("{0} \n\n", e.Message);
//    //Debug.WriteLine(message);
//    Console.WriteLine(message);
//}