using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QMS.ViewModels
{
    public class ReleaseViewModel
    {
        public long BatchId { get; set; }
        public string ExporterName { get; set; }
        public string Destination { get; set; }
        public string AnimalType { get; set; }
        public int OriginallyAccepted { get; set; }
        public int ReadyForRelease { get; set; }
        public int DisposedAnimals { get; set; }
        public int PendingAnimals { get; set; }
        public string RegistrationDate { get; set; }
        public int QuarantinePeriod { get; set; }
        public string QuarantineEndDate { get; set; }
        public string Message { get; set; }

    }
}