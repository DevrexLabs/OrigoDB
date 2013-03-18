using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OrigoDB.Core.Test
{
    [TestClass]
    public class Assumptions
    {
        private class MyImmutableCommand : Command, IImmutable
        {
            internal override void PrepareStub(Model model)
            {
                throw new NotImplementedException();
            }

            internal override object ExecuteStub(Model model)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void Is_operator_is_polymorphic()
        {
            Command command = new MyImmutableCommand();
            Assert.IsTrue(command is IImmutable);
        }
    }
}
