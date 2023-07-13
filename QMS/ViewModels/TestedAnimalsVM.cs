using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QMS.ViewModels
{
    public class TestedAnimalsVM
    {
        public DateTime DateEntered { get; set; }
        public string AnimalTag { get; set; }
        public string RangeFrom { get; set; }
        public string RangeTo { get; set; }
        public string Result { get; set; }
        public bool IsNew { get; set; }

        public long SystemId { get; set; }

        public string IsNewPlus
        {
            get
            {
                return (IsNew) ? "+" : "";
            }
        }

        public string FormattedDateEntered
        {
            get
            {
                return (DateEntered == null) ? "" : DateEntered.ToString("dd/MM/yyyy");
                
            }
        }
    }
}