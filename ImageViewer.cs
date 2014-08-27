using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace ViewerIntegration
{
    class ImageViewer
    {
        // TODO improve initialization of Pacs! especially string to boolean parsing!
        public static ViewerIntegration.ServerNode FindServer(string nameOfServer, string nameOfServerGroup = null)
        {
            if (nameOfServer == "My Studies")
                return new ViewerIntegration.ServerNode("My Studies", "", 0, false, true);
            
            var query = GetServerQuery(nameOfServer, nameOfServerGroup);

            if (query != null)
            {
                try
                {
                    XElement onePacs = query.SingleOrDefault();
                    if (onePacs != null)
                    {
                        Console.WriteLine("\nPacs\n");
                        Console.WriteLine("AETitle: {0}", onePacs.Element("AETitle").Value);
                        Console.WriteLine("Host: {0}", onePacs.Element("Host").Value);
                        Console.WriteLine("Port: {0}", onePacs.Element("Port").Value);
                        Console.WriteLine("IsStreaming: {0}", onePacs.Element("IsStreaming").Value);
                        //Console.WriteLine("Location: {0}", onePacs.Element("Location").Value);

                        bool isStreamingServer;
                        Boolean.TryParse(onePacs.Element("IsStreaming").Value, out isStreamingServer);

                        return new ViewerIntegration.ServerNode(onePacs.Element("AETitle").Value, onePacs.Element("Host").Value, int.Parse(onePacs.Element("Port").Value), isStreamingServer, false);

                        //select new Pacs(pacs.Element("AETitle").Value, pacs.Element("Host").Value, pacs.Element("Port").Value, bool.TryParse(pacs.Element("IsStreaming").Value))

                        //var projected = from ev in events
                        //                select new
                        //                {
                        //                    Title = ev.Title,
                        //                    StartTime = ev.StartTime,
                        //                    EndTime = ev.StartTime + ev.Duration
                        //                };
                    }
                    else
                    {
                        Console.WriteLine("Server '{0}' not found.", nameOfServer);
                        return null;
                    }
                }
                catch (InvalidOperationException invalidOperation)
                {
                    Console.WriteLine("Ambiguous server name '{0}'. Multiple server nodes were found. Please specify the server group name. " + invalidOperation.Message, nameOfServer);
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private static IEnumerable<XElement> GetServerQuery(string nameOfServer, string nameOfServerGroup = null)
        {
            string xmlDicomAEServers = Path.Combine(ViewerIntegrationSettings.Default.ViewerWorkingDirectory, ViewerIntegrationSettings.Default.ViewerDicomAEServersFile);
            XDocument imageViewerServerNodesXml = XDocument.Load(xmlDicomAEServers);

            // No ServerGroup name associated, search in all Server nodes. 
            // Will throw an exception if multiple Server nodes have the same name. Then a ServerGroup name has to be given.
            if (String.IsNullOrEmpty(nameOfServerGroup))
            {
                IEnumerable<XElement> query = from pacs in imageViewerServerNodesXml.Root.Element("ServerGroupNode").Descendants("Server")
                                              where pacs.Element("NameOfServer").Value == nameOfServer
                                              select pacs;
                return query;
            }
            else
            {

                XElement xelNameOfServerGroup = imageViewerServerNodesXml.Root.Element("ServerGroupNode").Descendants("NameOfGroup")
                                                    .Where(d => d.Value == nameOfServerGroup)
                                                    .FirstOrDefault();

                if (xelNameOfServerGroup != null)
                {
                    XElement xelServerGroup = xelNameOfServerGroup.Parent;

                    IEnumerable<XElement> query = from pacs in xelServerGroup.Element("ChildServers").Descendants("Server")
                                                  where pacs.Element("NameOfServer").Value == nameOfServer
                                                  select pacs;
                    return query;
                }
                else
                {
                    Console.WriteLine("ServerGroup named '{0}' not found!\n", nameOfServerGroup);
                    return null;
                }
            }

            return null;
        }

        public static string GetOwnAetitle()
        {
            string xmlDicomAEServers = Path.Combine(ViewerIntegrationSettings.Default.ViewerWorkingDirectory, ViewerIntegrationSettings.Default.ViewerDicomAEServersFile);
            XDocument imageViewerServerNodesXml = XDocument.Load(xmlDicomAEServers);

            return (imageViewerServerNodesXml.Root.Element("LocalDataStoreNode").Descendants("OfflineAE")).FirstOrDefault().Value;
        }
    }
}
