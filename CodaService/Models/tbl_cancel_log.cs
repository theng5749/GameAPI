//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class tbl_cancel_log
    {
        public int Id { get; set; }
        public string Gamename { get; set; }
        public string Msisdn { get; set; }
        public string Username { get; set; }
        public string ResultCode { get; set; }
        public string ResultDesc { get; set; }
        public Nullable<System.DateTime> CancelDate { get; set; }
        public string Channel { get; set; }
        public string USSDCode { get; set; }
    }
}
