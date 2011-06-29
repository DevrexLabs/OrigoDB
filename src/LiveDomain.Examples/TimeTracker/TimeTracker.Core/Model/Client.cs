using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace TimeTracker.Core
{
    [Serializable]
    public class Client
    {
        [Range(0, Int32.MaxValue)]
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; }

        public Client()
        {
        }
    }
}
