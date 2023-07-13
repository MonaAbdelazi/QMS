using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QMS.Data;

namespace QMS.ViewModels
{
    public class ClinicCaseVM : CLINIC_CASES
    {
        private QMSEntities db = new QMSEntities();

        public bool DisposedBool
        {
            get
            {
                return this.DISPOSED == "Yes";                
            }
            set
            {
                this.DISPOSED = (value) ? "Yes" : "No";
            }
        }

        private DIAGNOSES diagnosisObject;
        public DIAGNOSES DiagnosisObject
        {
            get
            {
                if (diagnosisObject == null)
                {
                    diagnosisObject = db.DIAGNOSES.Find(this.Diagnosis);
                }
                return diagnosisObject;
            }
        }


    }
}