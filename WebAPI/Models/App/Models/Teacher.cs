﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.App
{
    public class Teacher
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public int SchoolId { get; set; }
    }
}