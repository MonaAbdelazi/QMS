using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QMS.Data;


namespace QMS.ViewModels
{
    public class TestRecordViewModel
    {
        public Batchs BatchDetails { get; set; }
        public TESTS TestDetails { get; set; }
        public TEST_SUMMARY Summary { get; set; }

        public long BatchId { get; set; }
        public long TestId { get; set; }
        public string RangeFrom { get; set; }
        public string RangeTo { get; set; }


    }
}