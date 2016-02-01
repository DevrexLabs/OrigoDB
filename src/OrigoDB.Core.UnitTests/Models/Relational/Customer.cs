using System;
using OrigoDB.Core.Modeling.Relational;

namespace OrigoDB.Test.Models
{
    [Serializable]
    public class Customer : Entity
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }
}