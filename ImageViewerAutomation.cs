using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using ViewerIntegration.DicomExplorerAutomation;
using ViewerIntegration.StudyLocator;
using ViewerIntegration.ViewerAutomation;
using System.ServiceModel;

namespace ViewerIntegration
{
    /// <summary>
    /// Public class <see cref="ImageViewerAutomation"/> xxxx make use of the ClearCanvas automation services.
    /// </summary>
    public class ImageViewerAutomation
    {
        private const string regexAutomationPattern = @"&action=(?<action>.*)&parameter=(?<parameter>.*)(?:&servername=(?<servername>.*)&servergroup=(?<servergroup>.*)|&servername=(?<servername>.*))";
        private static UriScheme uriAutomationScheme = new UriScheme(regexAutomationPattern);

        /// <summary>
        /// Translates a URL into an <see cref="IImageViewerAutomationAction"/> object.
        /// </summary>
        /// <exception cref="System.ArgumentException">Thrown when <paramref name="urlToParse"/> does not match <see cref="UriScheme.TotalPattern()"/> </exception>
        /// <param name="urlToParse">URL which will be translated into an instance of <see cref="ImageViewerAction"/></param>
        /// <returns>Instance of <see cref="IImageViewerAutomationAction"/></returns>
        //public static ImageViewerAction ParseUrl(string urlToParse)
        public static IImageViewerAutomationAction ParseUrl(string urlToParse)
        {

            // TODO error check in URL - not sure what exactly that means??? guess it means here in ParseURL
            // TODO check what happens if servergroup = null? after making servergroup optional in UrlPattern
            // TODO parseURL should this move to UrlPattern??? not sure

            List<IImageViewerAutomationAction> listOfActions = new List<IImageViewerAutomationAction>();
            listOfActions.Add(new Query());

            try
            {
                GroupCollection matchedGroups = uriAutomationScheme.ParseUrl(urlToParse);

                Console.WriteLine("UriScheme.ParseUrl() found\n");
                foreach (string groupName in uriAutomationScheme.UrlValidationGroupNames())
                {
                    Console.WriteLine(
                       "Group: {0}, Value: {1}",
                       groupName,
                       matchedGroups[groupName].Value);
                }
                
                // TODO reaction to if one of the three is null!
                string matchedActionType = matchedGroups["action"].Value;
                ServerNode matchedServerNode = ImageViewer.FindServer(matchedGroups["servername"].Value, matchedGroups["servergroup"].Value);
                string matchedActionParameter = matchedGroups["parameter"].Value;               

                foreach (IImageViewerAutomationAction action in listOfActions)
                {
                    if (action.RegexPatternActionType == matchedActionType)
                    {
                        IImageViewerAutomationAction imageViewerAutomationAction = action.SetAction(matchedServerNode, matchedActionParameter);
                        //imageViewerAutomationAction.ActionOnServer = matchedServerNode;
                        return imageViewerAutomationAction;
                    }
                }

                return null;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        /// <summary>
        /// Opens the ClearCanvas ImageViewer when not already open
        /// </summary>
        public static void OpenImageViewer(Boolean waitForServiceToOpen = true)
        {
            Process[] viewerProcesses = Process.GetProcessesByName(ViewerIntegrationSettings.Default.ViewerProcessExecutable);
            if (viewerProcesses == null || viewerProcesses.Length == 0)
            {
                string viewerProcessPath = ViewerIntegrationSettings.Default.ViewerWorkingDirectory;
                if (!Path.IsPathRooted(viewerProcessPath))
                    viewerProcessPath = Path.Combine(Directory.GetCurrentDirectory(), viewerProcessPath);

                string executable = Path.Combine(viewerProcessPath, ViewerIntegrationSettings.Default.ViewerProcessExecutable);
                executable += ".exe";

                ProcessStartInfo startInfo = new ProcessStartInfo(executable, "");
                startInfo.WorkingDirectory = viewerProcessPath;
                Process viewerProcess = Process.Start(startInfo);
                if (viewerProcess == null)
                {
                    Console.WriteLine("Failed to start the viewer process.");
                }
                else
                {
                    if (waitForServiceToOpen)
                    {
                        int numberOfAttempts = ViewerIntegrationSettings.Default.ViewerReachableNumberOfAttempts;
                        int delayAttempsInMilliSeconds = ViewerIntegrationSettings.Default.ViewerReachableDelayAttempsInMilliSeconds;
                        Boolean seviceReachable = false;

                        int i = 0;
                        while (i < numberOfAttempts && seviceReachable == false)
                        {
                            seviceReachable = IsServiceReachable();

                            if (seviceReachable == false)
                            {
                                i += 1;
                                System.Threading.Thread.Sleep(delayAttempsInMilliSeconds);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests if the ViewerAutomation service is reachable.
        /// </summary>
        public static Boolean IsServiceReachable()
        {
            // TODO change name of function? or adapt it so that all three service channels can be tested or that it accepts a automation client to be tested as parameter?

            using (ViewerAutomationClient viewerAutomationClient = new ViewerAutomationClient("ViewerAutomation"))
            {
                viewerAutomationClient.Open();

                try
                {
                    // get all open viewers  
                    ViewerAutomation.GetViewersResult resultViewers = viewerAutomationClient.GetViewers(new ViewerAutomation.GetViewersRequest());
                    return true;
                }
                catch (FaultException<NoViewersFault> noViewersFault)
                {
                    //Console.WriteLine("Client reachable at attempt {0}/{1}.", i + 1, numberOfAttempts);
                    return true;
                }
                catch (FaultException unknownFault)
                {
                    //Console.WriteLine("An unknown exception was received. " + unknownFault.Message);
                    viewerAutomationClient.Abort();
                }
                catch (EndpointNotFoundException serviceOffline)
                {
                    //Console.WriteLine("The service is not yet reachable. Going for attempt {0}/{1}", i + 1, numberOfAttempts);
                    viewerAutomationClient.Abort();
                }
                catch (TimeoutException timeProblem)
                {
                    //Console.WriteLine("The service operation timed out. " + timeProblem.Message);
                    viewerAutomationClient.Abort();
                }
                catch (CommunicationException commProblem)
                {
                    //Console.WriteLine("There was a communication problem. " + commProblem.Message + commProblem.StackTrace);
                    viewerAutomationClient.Abort();
                }
                catch (Exception e)
                {
                    //Console.WriteLine("There was a communication problem. " + commProblem.Message + commProblem.StackTrace);
                    viewerAutomationClient.Abort();
                }
                finally
                {
                    viewerAutomationClient.Close();
                }
            }

            return false;
        }


        public static Boolean Execute(ImageViewerAction imageViewerAction)
        {
            //TODO discern between local/remote retrieve? (simple ClearCanvas ImageViewer automation vs C-Move and then ClearCanvas ImageViewer automation)
            try
            {
                // Launch the ClearCanvas ImageViewer if not already open
                OpenImageViewer();

                switch (imageViewerAction.Type)
                {
                    case "query":
                        imageViewerAction.ExecuteQuery();
                        break;
                    case "retrieve":
                        imageViewerAction.ExecuteRetrieve();
                        break;
                    case "open":
                        imageViewerAction.ExecuteOpen();
                        break;
                    default:
                        Console.WriteLine("Error: URL requested non-defined action!");
                        return false;
                }
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }

        //public static void StartViewer()
        //{
        //    const string processName = "ClearCanvas.Desktop.Executable";
        //    const string viewerProcessPath = @"C:\Program Files\ClearCanvas\ClearCanvas Workstation";

        //    Process[] viewerProcesses = Process.GetProcessesByName(processName);
        //    if (viewerProcesses == null || viewerProcesses.Length == 0)
        //    {
        //        string executable = Path.Combine(viewerProcessPath, processName) + ".exe";

        //        ProcessStartInfo startInfo = new ProcessStartInfo(executable, "");
        //        startInfo.WorkingDirectory = viewerProcessPath;
        //        Process viewerProcess = Process.Start(startInfo);
        //        if (viewerProcess == null)
        //            Console.WriteLine("Failed to start the viewer process.");
        //    }
        //}
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