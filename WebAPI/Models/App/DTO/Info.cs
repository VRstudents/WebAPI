﻿using System.Collections.Generic;

namespace WebAPI.Models.App
{
    public class Info
    {
        public int lessonsCompleted { get; set; }
        public int qCorrAnswered { get; set; }
        public List<LessonDTO> best { get; set; }
        public List<LessonDTO> worst { get; set; }
    }
}