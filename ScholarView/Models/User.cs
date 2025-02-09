using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScholarView.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public System.DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public RoleEnum Role { get; set; }
        public byte[] UserPhoto { get; set; }
        public string Password { get; set; }
    }
}

