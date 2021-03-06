﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BridgeCare.EntityClasses
{
    [Table("ATTRIBUTES_")]
    public class AttributesEntity
    {
        [Key]
        public string ATTRIBUTE_ { get; set; }
        public bool? ASCENDING { get; set; }
        public string Type_ { get; set; }
    }
}