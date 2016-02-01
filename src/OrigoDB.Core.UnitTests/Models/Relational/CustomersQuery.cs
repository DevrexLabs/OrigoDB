using System;
using System.Collections.Generic;
using System.Linq;
using OrigoDB.Core;
using OrigoDB.Core.Modeling.Relational;

namespace OrigoDB.Test.Models
{
    [Serializable]
    public class CustomersQuery : Query<RelationalModel, List<string>>
    {

        public readonly string Prefix;

        public CustomersQuery(string prefix)
        {
            Prefix = prefix;
        }

        public override List<String> Execute(RelationalModel model)
        {
            return model
                .From<Customer>()
                .Where(c => c.Name.StartsWith(Prefix)).Select(c => c.Name)
                .ToList();
        }
    }
}