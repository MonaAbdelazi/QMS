using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QMS.Data;

namespace QMS.ViewModels
{
    public class DiagnosesVM : DIAGNOSES
    {
        public bool RecommendDisposalBool
        {
            get
            {
                return RECOMMEND_DISPOSAL == "true";
            }
            set
            {
                RECOMMEND_DISPOSAL = (value) ? "true" : "false";
            }
        }
    }
}