//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QMS.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class DESTINATION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DESTINATION()
        {
            this.Batchs = new HashSet<Batchs>();
            this.DESTINATION_TESTS = new HashSet<DESTINATION_TESTS>();
            this.QUARENTINE_PERIOD = new HashSet<QUARENTINE_PERIOD>();
            this.ReleaseRecord = new HashSet<ReleaseRecord>();
        }
    
        public long SYSTEMID { get; set; }
        public string DESTINATION_NAME { get; set; }
        public string NOTES { get; set; }
        public string STATUS { get; set; }
        public string CREATED_BY { get; set; }
        public string AUTHORIZED_BY { get; set; }
        public Nullable<long> LAST_UPDATED_BY { get; set; }
        public System.DateTime CREATION_DATE { get; set; }
        public System.DateTime AUTHORIZATION_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Batchs> Batchs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DESTINATION_TESTS> DESTINATION_TESTS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<QUARENTINE_PERIOD> QUARENTINE_PERIOD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReleaseRecord> ReleaseRecord { get; set; }
    }
}
