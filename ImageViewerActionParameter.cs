using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ClearCanvas.Dicom;
using ViewerIntegration.DicomExplorerAutomation;

namespace ViewerIntegration
{
    public class ImageViewerActionParameter
    {
        public readonly string level;
        public readonly DicomTag dicomTag;
        public readonly string dicomValue;

        // Constructor initializing Query with all its fields
        public ImageViewerActionParameter(string levelPar, string dicomTagPar, string dicomValuePar)
        {
            level = levelPar;
            dicomTag = DicomTagDictionary.GetDicomTag(dicomTagPar);
            if (dicomTag == null)
                throw new ArgumentOutOfRangeException("Invalid DICOM tag: " + dicomTagPar);
            
            dicomValue = dicomValuePar;
        }

        /// <summary>
        /// Converts this object into a <see cref="DicomAttributeCollection"/>.
        /// </summary>
        public DicomExplorerSearchCriteria ToDicomExplorerSearchCriteria()
        {
            // DICOM Tags to query
            // studyCriteria.AccessionNumber
            // studyCriteria.PatientId
            // studyCriteria.PatientsName
            // studyCriteria.ReferringPhysiciansName
            // studyCriteria.StudyDescription

            // NON-DICOM Tags to query
            // studyCriteria.Modalities (relates to ModalitiesInStudy or Modality)
            // studyCriteria.StudyDateFrom (relates to StudyDate)
            // studyCriteria.StudyDateTo (relates to StudyDate)
            //studyCriteria.PatientId = GetPatientId(actionParameter.dicomTag, actionParameter.dicomValue);

            var searchCriteria = new DicomExplorerSearchCriteria();
            if (!string.IsNullOrEmpty(dicomValue))
                searchCriteria.AccessionNumber = "";

            //attributes[DicomTags.QueryRetrieveLevel].SetStringValue(QueryRetrieveLevel);
            //attributes.SaveDicomFields(this);

            return searchCriteria;
        }
    }
}

//// OLD IMPLEMENTATION where queryParam simply relates to any DICOM Tag
////public readonly string level;
////public readonly DicomTag dicomTag;
////public readonly string dicomValue;

////// Constructor initializing Query with all its fields
////public ImageViewerActionParameter(string levelPar, string dicomTagPar, string dicomValuePar)
////{
////    level = levelPar;
////    dicomTag = DicomTagDictionary.GetDicomTag(dicomTagPar);
////    if (dicomTag == null)
////        throw new ArgumentOutOfRangeException("Invalid DICOM tag: " + dicomTagPar);

////    dicomValue = dicomValuePar;
////}