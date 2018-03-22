using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Configuration;

namespace WebApplication1.Models.App
{
    public class DBModel : DbContext
    {
        public DBModel() : base(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<StudentToClasses> StudentsToClasses { get; set; }
        public DbSet<User> Users { get; set; }
    }
}