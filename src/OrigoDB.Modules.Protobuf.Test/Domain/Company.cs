using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Modules.ProtoBuf.Test.Domain
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
    internal class Company
    {
       // [ProtoMember(1)]
        public string Name { get; set; }
       // [ProtoMember(2)]
        public List<Employee> Employees { get; set; }
    }
}
