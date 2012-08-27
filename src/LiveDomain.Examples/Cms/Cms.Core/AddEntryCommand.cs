using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cms.Core;
using LiveDomain.Core;

namespace Blog.Core
{

    [Serializable]
    public class AddEntryCommand : Command<BlogModel>
    {

        protected override void Execute(BlogModel model)
        {
            throw new NotImplementedException();
        }
    }


}
