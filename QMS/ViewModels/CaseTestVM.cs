using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QMS.ViewModels
{
    public class CaseTestVM
    {
        public long SYSTEMID { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }

        public bool NewModel { get; set; } = false;
    }
}