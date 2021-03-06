﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain
{
    public class ExerciseGroup
    {
        public Guid ExerciseId { get; set; }
        [ForeignKey("ExerciseId")]
        public virtual Exercise Exercise { get; set; }

        public Guid GroupId { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; }
    }
}
