//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScholarView
{
    using System;
    using System.Collections.Generic;
    
    public partial class Enrollment
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public Nullable<decimal> Grade { get; set; }
    
        public virtual Cours Cours { get; set; }
        public virtual User User { get; set; }
    }
}
