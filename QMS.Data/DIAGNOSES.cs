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
    
    public partial class DIAGNOSES
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public DIAGNOSES()
        {
            this.CASE_DIAGNOSIS = new HashSet<CASE_DIAGNOSIS>();
        }
    
        public long SYSTEMID { get; set; }
        public string SHORT_DESCRIPTION { get; set; }
        public string LONG_DESCRIPTION { get; set; }
        public string RECOMMEND_DISPOSAL { get; set; }
        public string STATUS { get; set; }
        public string CREATED_BY { get; set; }
        public string AUTHORIZED_BY { get; set; }
        public string LAST_UPDATED_BY { get; set; }
        public System.DateTime CREATION_DATE { get; set; }
        public Nullable<System.DateTime> AUTHORIZATION_DATE { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE_DATE { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CASE_DIAGNOSIS> CASE_DIAGNOSIS { get; set; }
    }
}
