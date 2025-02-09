using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace ScholarView
{
   

    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Cours> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        public ApplicationDbContext() : base("name=ScholarViewDBEntities")
        {
        }
    }


}
