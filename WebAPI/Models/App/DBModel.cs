using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Configuration;

namespace WebAPI.Models.App
{
    public class DBModel : DbContext
    {
        public DBModel() : base(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString) { }

        public DbSet<ClassGroup> ClassGroups { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<LessonsToClass> LessonsToClasses { get; set; }
        public DbSet<School> Schools { get; set; }
        public DbSet<ProgressInClass> ProgressInClasses { get; set; }
        public DbSet<ResultInLesson> ResultInLessons { get; set; }
        public DbSet<ResultInQuestion> ResultInQuestions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<StudentToClasses> StudentsToClasses { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<TeacherToClasses> TeachersToClasses { get; set; }
        public DbSet<User> Users { get; set; }
    }
}