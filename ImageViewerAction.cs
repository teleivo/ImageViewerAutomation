using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using ViewerIntegration.DicomExplorerAutomation;
using ViewerIntegration.StudyLocator;
using ViewerIntegration.ViewerAutomation;
using System.Reflection;

using ClearCanvas.Dicom.Network.Scu;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.Common;
using ClearCanvas.Dicom;

namespace ViewerIntegration
{
    public class ImageViewerAction
    {
        //TODO call parsing URL method in the constructor or do it from elsewhere?

        // Field
        //private readonly string url;
        private readonly string type;
        private readonly ImageViewerActionParameter actionParameter;
        private readonly ServerNode executeActionOnServer;

        // Constructor that takes 4 arguments.
        public ImageViewerAction(string urlPar, string typePar, ImageViewerActionParameter actionParameterPar, ServerNode serverPar)
        {
            //url = urlPar;
            type = typePar;
            actionParameter = actionParameterPar;
            executeActionOnServer = serverPar;
        }

        //public string Url
        //{
        //    get
        //    {
        //        return url;
        //    }
        //}

        public string Type
        {
            get
            {
                return type;
            }
        }

        public Boolean ExecuteQuery()
        {
            if (executeActionOnServer.isLocal)
                return ExecuteQueryLocal();
            else
                return ExecuteQueryRemote();
        }

        
        public Boolean ExecuteQueryRemote()
        {
            //TODO what to do when request comes in with a Study Instance UID? Get PatientId and maybe Accession Nr 

            DicomExplorerAutomationClient dicomExplorerAutomationClient = new DicomExplorerAutomationClient("DicomExplorerAutomation");
            
            //SearchRemoteStudiesRequest studySearchRemote = new SearchRemoteStudiesRequest();
            ////DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();
            //DicomExplorerSearchCriteria studyCriteria = actionParameter.ToDicomExplorerSearchCriteria();
            //studySearchRemote.AETitle = executeActionOnServer.aeTitle;
            //studySearchRemote.SearchCriteria = studyCriteria;

            SearchRemoteStudiesRequest studySearchRemote = new SearchRemoteStudiesRequest() {   SearchCriteria = actionParameter.ToDicomExplorerSearchCriteria(), 
                                                                                                AETitle = executeActionOnServer.aeTitle };
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
            SearchLocalStudiesRequest studySearchLocal = new SearchLocalStudiesRequest();
            DicomExplorerSearchCriteria studyCriteria = new DicomExplorerSearchCriteria();

            studyCriteria.PatientId = GetPatientId(actionParameter.dicomTag, actionParameter.dicomValue);

            studySearchLocal.SearchCriteria = studyCriteria;
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

        public Boolean ExecuteRetrieve()
        {

            //// Connect to the ClearCanvas ImageViewer Automation service
            //ViewerAutomationClient viewerAutomationClient = new ViewerAutomationClient("ViewerAutomation");

            //// OPENS LOCAL/ImageServer STUDY IN VIEWER!
            //OpenStudyInfo info = new OpenStudyInfo();
            //info.StudyInstanceUid = query.dicomValue;
            //OpenStudyInfo[] studiesToOpen = new OpenStudyInfo[] { info };
            //OpenStudiesRequest request = new OpenStudiesRequest();
            //request.StudiesToOpen = studiesToOpen;
            

            //request.ActivateIfAlreadyOpen = true;

            //try
            //{
            //    viewerAutomationClient.OpenStudies(request);
            //    viewerAutomationClient.Close();
            //}
            //catch (Exception e)
            //{
            //    string message = string.Format("{0} \n\n", e.Message);
            //    Console.WriteLine(message);
            //    return false;
            //}

            try
            {
                string imageViewerAetitle = ImageViewer.GetOwnAetitle();
                DicomCMove(ViewerIntegrationSettings.Default.MoveScuLocalAe, executeActionOnServer, imageViewerAetitle, actionParameter.dicomValue);            
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;

        }

        // TODO check first if the study is already locally stored? but if so what tells us it is equal to the one on the server?
        //      imagine, you want to get a specific Study 1h ago, now there is a new series included in that study, so this one should be downloaded again
        //      need to delete the one I have locally first? Or does the ImageViewer merge those two anyway.
        // TODO check case: new series was added to a study on the pacs that I already retrieved, retrieve again. what happens?
        public Boolean ExecuteOpen()
        {

            if (executeActionOnServer.isStreaming == false)
            {

                // issue a C-MOVE request to the PACS to send the studies of interest to the ClearCanvas ImageViewer Workstation
                ExecuteRetrieve();
            }

            // Connect to the ClearCanvas ImageViewer Automation service
            ViewerAutomationClient viewerAutomationClient = new ViewerAutomationClient("ViewerAutomation");

            // OPENS LOCAL/ImageServer STUDY IN VIEWER!
            OpenStudyInfo openStudyInfo = new OpenStudyInfo();

            openStudyInfo.StudyInstanceUid = actionParameter.dicomValue;

            OpenStudyInfo[] studiesToOpen = new OpenStudyInfo[] { openStudyInfo };
            OpenStudiesRequest request = new OpenStudiesRequest() { StudiesToOpen = studiesToOpen, ActivateIfAlreadyOpen = true };

            try
            {
                viewerAutomationClient.OpenStudies(request);
                viewerAutomationClient.Close();
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }

        private Boolean StudyLocator()
        {

            // Connect to the ClearCanvas ImageViewer Automation service
            StudyRootQueryClient studyLocatorClient = new StudyRootQueryClient("StudyLocator");

            StudyRootStudyIdentifier requestStudyRootQuery = new StudyRootStudyIdentifier();
            requestStudyRootQuery.StudyInstanceUid = actionParameter.dicomValue;

            StudyRootStudyIdentifier[] result;

            try
            {
                result = studyLocatorClient.StudyQuery(requestStudyRootQuery);
                if (result != null)
                {

                    Type type = result[0].GetType();
                    PropertyInfo[] properties = type.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        Console.WriteLine("Name: " + property.Name + ", Value: " + property.GetValue(result[0], null));
                    }
                }

                studyLocatorClient.Close();
            }
            catch (Exception e)
            {
                string message = string.Format("{0} \n\n", e.Message);
                Console.WriteLine(message);
                return false;
            }

            return true;
        }

        private string GetPatientId(DicomTag queryDicomTag, string queryDicomValue)
        {
            StudyRootQueryClient studyLocatorClient = new StudyRootQueryClient("StudyLocator");
            StudyRootStudyIdentifier studyRootStudyIdentifier = new StudyRootStudyIdentifier();
            DicomAttributeCollection studyQueryDicomAttributes = studyRootStudyIdentifier.ToDicomAttributeCollection();

            // is DicomTag given contained available in StudyRootStudyIdentifier
            if (studyQueryDicomAttributes.Contains(queryDicomTag))
            {
                // set the value to the appropriate tag in studyRootStudyIdentifier
                PropertyInfo propertyInfo = studyRootStudyIdentifier.GetType().GetProperty(queryDicomTag.VariableName);
                propertyInfo.SetValue(studyRootStudyIdentifier, queryDicomValue, null);               
            }
            else
            {
                throw new ArgumentOutOfRangeException("StudyRootStudyIdentifier does not contain query tag " + queryDicomTag.Name);
            }

            studyLocatorClient.Open();

            //IList<StudyRootStudyIdentifier> studyRootStudyIdentifierResults = studyLocatorClient.StudyQuery(identifier);
            StudyRootStudyIdentifier[] studyRootStudyIdentifierResults = studyLocatorClient.StudyQuery(studyRootStudyIdentifier);
            studyLocatorClient.Close();

            if (studyRootStudyIdentifierResults != null)
                if (studyRootStudyIdentifierResults.Length > 0)
                    return studyRootStudyIdentifierResults[0].PatientId;
                else
                    return null;
            else
                return null;
        }

        private static void DicomCMove(string moveScuLocalAe, ServerNode moveScuRemoteAe, string moveScuMoveDestinationAe, string studyInstanceUID)
        {
            int myImportantVariable = 0;
            MoveScuBase moveScu;

            //using (MoveScuBase moveScu = new StudyRootMoveScu(moveScuLocalAe, moveScuRemoteAe.aeTitle, moveScuRemoteAe.ipAddress, moveScuRemoteAe.port, moveScuMoveDestinationAe))
            //{
            //    moveScu.AddStudyInstanceUid(studyInstanceUID);
            //    moveScu.BeginMove(new AsyncCallback(OnDataReceived), myImportantVariable);
            //}

            if (true)
            {
                moveScu = new StudyRootMoveScu(moveScuLocalAe, moveScuRemoteAe.aeTitle, moveScuRemoteAe.ipAddress, moveScuRemoteAe.port, moveScuMoveDestinationAe);
            }
            else
            {
                moveScu = new PatientRootMoveScu(moveScuLocalAe, moveScuRemoteAe.aeTitle, moveScuRemoteAe.ipAddress, moveScuRemoteAe.port, moveScuMoveDestinationAe);
            }

            moveScu.AddStudyInstanceUid(studyInstanceUID);

            // TODO add this nice to have, using the DicomTags as well
            // NICE TO HAVE
            //if (queryMessage.Contains(DicomTags.PatientId))
            //{
            //    var array = queryMessage[DicomTags.PatientId].Values as string[];
            //    if (array != null)
            //        foreach (string s in array)
            //            moveScu.AddPatientId(s);
            //}
            //if (queryMessage.Contains(DicomTags.StudyInstanceUid))
            //{
            //    var array = queryMessage[DicomTags.StudyInstanceUid].Values as string[];
            //    if (array != null)
            //        foreach (string s in array)
            //            moveScu.AddStudyInstanceUid(s);
            //}
            //if (queryMessage.Contains(DicomTags.SeriesInstanceUid))
            //{
            //    var array = queryMessage[DicomTags.SeriesInstanceUid].Values as string[];
            //    if (array != null)
            //        foreach (string s in array)
            //            moveScu.AddSeriesInstanceUid(s);
            //}
            //if (queryMessage.Contains(DicomTags.SopInstanceUid))
            //{
            //    var array = queryMessage[DicomTags.SopInstanceUid].Values as string[];
            //    if (array != null)
            //        foreach (string s in array)
            //            moveScu.AddSopInstanceUid(s);
            //}

            
            moveScu.ImageMoveCompleted += delegate(object o, EventArgs args)
            {
                var eventScu = o as MoveScuBase;
                if (eventScu != null)
                {
                    //Platform.Log(LogLevel.Info,
                    //                "Total SubOps: {0}, Remaining SubOps {1}, Success SubOps: {2}, Failure SubOps: {3}, Warning SubOps: {4}, Failure Description: {5}",
                    //                eventScu.TotalSubOperations,
                    //                eventScu.RemainingSubOperations,
                    //                eventScu.SuccessSubOperations,
                    //                eventScu.FailureSubOperations,
                    //                eventScu.WarningSubOperations,
                    //                eventScu.FailureDescription);
                    Console.WriteLine("Total SubOps: {0}, Remaining SubOps {1}, Success SubOps: {2}, Failure SubOps: {3}, Warning SubOps: {4}, Failure Description: {5}",
                        eventScu.TotalSubOperations,
                        eventScu.RemainingSubOperations,
                        eventScu.SuccessSubOperations,
                        eventScu.FailureSubOperations,
                        eventScu.WarningSubOperations,
                        eventScu.FailureDescription);
                }
            };
            //moveScu.BeginMove(delegate
            //{
            //    Invoke(new Action<string>(delegate { buttonMoveScuMove.Text = "Move"; }), new object[] { "Move" });
            //}, this);
            moveScu.BeginMove(new AsyncCallback(OnDataReceived), myImportantVariable);

        }

        private static void OnDataReceived(IAsyncResult result)
        {
            Console.WriteLine("C-MOVE async result state: {0}", result.AsyncState);
        }

    }

}
