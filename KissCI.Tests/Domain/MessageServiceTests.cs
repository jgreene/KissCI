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
using KissCI.NHibernate.Internal;

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
            SessionManager.InitDb();


            using (var dataProvider = new NHibernateDataContext())
            {
                var service = dataProvider.TaskMessageService;

                var message1 = new TaskMessage
                {
                    Time = TimeHelper.Now,
                    BuildId = 0,
                    Message = "This is a test message"
                };

                var message2 = new TaskMessage
                {
                    Time = TimeHelper.Now,
                    BuildId = 0,
                    Message = "This is a test message"
                };

                service.WriteMessage(message1);
                service.WriteMessage(message2);

                var messages = service.GetMessages();

                var count = messages.Count();

                Assert.IsTrue(count == 2);

                var read = messages.First();
                Assert.AreEqual(message1.Message, read.Message);

            }

            
        }
    }
}
