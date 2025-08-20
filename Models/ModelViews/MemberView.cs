using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WBookStore.Models.ModelViews
{
    public class MemberView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int Role { get; set; }
        public int Active { get; set; }
    }
}