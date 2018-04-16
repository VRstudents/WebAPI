using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class PersonDTO
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string SchoolName { get; set; }
        public int Grade { get; set; }
        public int Id { get; set; }
        public string Picture { get; set; }
    }
}