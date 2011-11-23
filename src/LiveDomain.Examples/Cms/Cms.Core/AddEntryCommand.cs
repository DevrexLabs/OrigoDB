using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cms.Core;

namespace Blog.Core
{

    [Serializable]
    public class AddEntryCommand : Command<BlogModel>
    {

        public override void Execute(BlogModel db)
        {
        }
    }


}
