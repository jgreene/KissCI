using KissCI.Internal.Domain;
using KissCI.Helpers;
using KissCI.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using KissCI.NHibernate;

namespace KissCI.Tests.Domain
{
    [TestClass]
    public class MessageServiceTests
    {
        [TestMethod]
        public void CanWriteAndReadMessages()
        {
            var executableDirectory = DirectoryHelper.CurrentDirectory();
            var outputDirectory = Path.Combine(executableDirectory.FullName, "MessageService");

            DirectoryHelper.CleanAndEnsureDirectory(outputDirectory);



            using (var dataProvider = new NHibernateDataContext())
            {
                var service = dataProvider.TaskMessageService;

                var message = new TaskMessage
                {
                    Time = TimeHelper.Now,
                    Message = "This is a test message"
                };

                service.WriteMessage(message);
                service.WriteMessage(message);

                var messages = service.GetMessages();

                Assert.IsTrue(messages.Count() == 2);

                var read = messages.First();
                Assert.AreEqual(message.Message, read.Message);

            }

            
        }
    }
}
