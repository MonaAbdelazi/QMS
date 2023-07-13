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
    
    public partial class CHEMICAL_DOSE
    {
        public long SYSTEMID { get; set; }
        public long CHEMICAL_ID { get; set; }
        public long ANIMAL_TYPE_ID { get; set; }
        public string WEIGHT_HEAD { get; set; }
        public decimal DOSE { get; set; }
        public string STATUS { get; set; }
        public long CREATED_BY { get; set; }
        public long AUTHORIZED_BY { get; set; }
        public long LAST_UPDATED_BY { get; set; }
        public System.DateTime CREATION_DATE { get; set; }
        public System.DateTime AUTHORIZATION_DATE { get; set; }
        public System.DateTime LAST_UPDATE_DATE { get; set; }
    
        public virtual ANIMAL_TYPE ANIMAL_TYPE { get; set; }
        public virtual CHEMICAL CHEMICAL { get; set; }
        public virtual USERS USERS { get; set; }
        public virtual USERS USERS1 { get; set; }
        public virtual USERS USERS2 { get; set; }
    }
}
