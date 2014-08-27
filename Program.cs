using System;
using System.Reflection;

namespace ViewerIntegration
{
    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 1)
            {
                Console.WriteLine("Calling with URL:\n" + args[0] + "\n");
                Console.WriteLine("Using regex pattern:\n" + UriSchemeValidator.TotalPattern() + "\n");
                
                // Initialize a viewerAction instance with the command line arg URL
                ImageViewerAction imageViewerAction = ImageViewerAutomation.ParseUrl(args[0]);

                if (imageViewerAction != null)
                {
                    // Execute the action defined in the URL in the ImageViewer
                    ImageViewerAutomation.Execute(imageViewerAction);
                }
            }
            else
            {
                Console.WriteLine("1 argument expected, %d argument(s) given.\n", args.Length);
                ShowUsage();
                return;
            }

            Console.WriteLine("Press any key to continue...");
            Console.Read();

        }
        
        static void ShowUsage()
        {
            Console.WriteLine("\\\\\\Usage " + Assembly.GetExecutingAssembly().GetName().Name);
            Console.WriteLine("Command line: " + Assembly.GetExecutingAssembly().GetName().Name + " \"{0}\" \n", UriSchemeValidator.TotalPattern());
        }

    }
}

// NOT SURE IF STILL NEEDED
// VARIANTE 1 ???
//viewerinfo = client.GetViewerInfo();
//GetViewerInfoRequest rqviewer = new GetViewerInfoRequest();
//GetViewerInfoResult viewerinfo = client.GetViewerInfo(rqviewer);
//Console.WriteLine(viewerinfo);

//GetActiveViewersResult result = client.GetActiveViewers();
//foreach (Viewer viewer in result.ActiveViewers)
//{

//    CloseViewerRequest cclose = new CloseViewerRequest();
//    cclose.Viewer = viewer;
//    cclose.Viewer.PrimaryStudyInstanceUid = uid;
//    client.CloseViewer(cclose);
//}

// VARIANTE 1
//List<OpenStudyInfo> studiesToOpen = new List<OpenStudyInfo>();
//OpenStudyInfo info = new OpenStudyInfo();
//info.StudyInstanceUid = studyInstanceUid;
//studiesToOpen.Add(info);

//OpenStudiesRequest request = new OpenStudiesRequest();
//request.StudiesToOpen = studiesToOpen;