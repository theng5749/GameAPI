﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CodaService.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CodaPayDBEntities : DbContext
    {
        public CodaPayDBEntities()
            : base("name=CodaPayDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbl_backlist> tbl_backlist { get; set; }
        public virtual DbSet<tbl_deduct_log> tbl_deduct_log { get; set; }
        public virtual DbSet<tbl_deduct_test> tbl_deduct_test { get; set; }
        public virtual DbSet<tbl_header> tbl_header { get; set; }
        public virtual DbSet<tbl_request_log> tbl_request_log { get; set; }
        public virtual DbSet<tbl_request_uat> tbl_request_uat { get; set; }
        public virtual DbSet<tbl_sms_log> tbl_sms_log { get; set; }
        public virtual DbSet<tbl_confirm_deduct> tbl_confirm_deduct { get; set; }
        public virtual DbSet<tbl_member> tbl_member { get; set; }
        public virtual DbSet<tbl_request_log_production> tbl_request_log_production { get; set; }
        public virtual DbSet<tbl_cancel_log> tbl_cancel_log { get; set; }
    }
}
