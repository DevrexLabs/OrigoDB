using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core
{
    [Serializable]
    public class Role
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        [Range(0, Double.MaxValue)]
        public decimal DefaultHourlyRate { get; set; }

        public Role() : this(0, String.Empty, String.Empty, 0) { }

        public Role(int id, String name, String description, decimal defaultHourlyRate)
        {
            if (name == null || description == null)
                throw new ArgumentNullException();

            Id = id;
            Name = name;
            Description = description;
            DefaultHourlyRate = defaultHourlyRate;
        }
    }
}