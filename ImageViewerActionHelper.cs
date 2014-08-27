using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClearCanvas.Dicom;
using ViewerIntegration.StudyLocator;
using ClearCanvas.Dicom.ServiceModel.Query;
using System.Reflection;

namespace ViewerIntegration
{
    public static class ImageViewerActionHelper
    {
        public static string GetPatientId(DicomTag queryDicomTag, string queryDicomValue)
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
    }
}
