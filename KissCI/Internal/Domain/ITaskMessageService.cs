using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace KissCI.Internal.Domain
{
    public interface ITaskMessageService
    {
        void WriteMessage(TaskMessage message);
        IQueryable<TaskMessage> GetMessagesForBuild(long buildId);
        IQueryable<TaskMessage> GetMessages();
    }
}
