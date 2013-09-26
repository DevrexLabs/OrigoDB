using NUnit.Framework;
using OrigoDB.Core.Logging;

namespace OrigoDB.Core.Test
{
    [TestFixture]
    public class LoggingTests
    {
        [Test]
        public void GetForCallingType_sets_correct_name()
        {
            LogProvider.SetFactory(new ConsoleLoggerFactory());
            ConsoleLogger logger = (ConsoleLogger) LogProvider.Factory.GetLoggerForCallingType();
            string expectedName = this.GetType().FullName;
            Assert.AreEqual(expectedName, logger.Name);
        }
    }
}