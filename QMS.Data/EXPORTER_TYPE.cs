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
    
    public partial class EXPORTER_TYPE
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public EXPORTER_TYPE()
        {
            this.EXPORTER = new HashSet<EXPORTER>();
        }
    
        public long SYSTEMID { get; set; }
        public string TYP_NAME { get; set; }
        public string INDIVIDUAL_CORPORATE { get; set; }
        public string NOTES { get; set; }
        public string STATUS { get; set; }
        public string CREATED_BY { get; set; }
        public Nullable<long> AUTHORIZED_BY { get; set; }
        public long LAST_UPDATED_BY { get; set; }
        public System.DateTime CREATION_DATE { get; set; }
        public System.DateTime AUTHORIZATION_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EXPORTER> EXPORTER { get; set; }
    }
}
