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
    
    public partial class CASE_DIAGNOSIS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CASE_DIAGNOSIS()
        {
            this.CLINIC_CASES = new HashSet<CLINIC_CASES>();
        }
    
        public long SYSTEMID { get; set; }
        public long CASE_ID { get; set; }
        public long DIAGNOSIS_ID { get; set; }
        public string STATUS { get; set; }
        public string CREATED_BY { get; set; }
        public string AUTHORIZED_BY { get; set; }
        public string LAST_UPDATED_BY { get; set; }
        public System.DateTime CREATION_DATE { get; set; }
        public Nullable<System.DateTime> AUTHORIZATION_DATE { get; set; }
        public Nullable<System.DateTime> LAST_UPDATE_DATE { get; set; }
    
        public virtual DIAGNOSES DIAGNOSES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CLINIC_CASES> CLINIC_CASES { get; set; }
        public virtual CLINIC_CASES CLINIC_CASES1 { get; set; }
    }
}