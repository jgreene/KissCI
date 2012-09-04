using KissCI.Internal.Domain;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace KissCI.NHibernate.Internal
{
    public class TaskMessageService : ITaskMessageService
    {
        public TaskMessageService(ISession session)
        {
            _session = session;
        }

        ISession _session;

        public void WriteMessage(TaskMessage message)
        {
            _session.Save(message);
        }

        public IQueryable<TaskMessage> GetMessages()
        {
            return _session.Query<TaskMessage>();
        }

        public IQueryable<TaskMessage> GetMessagesForBuild(int buildId)
        {
            return GetMessages().Where(tm => tm.BuildId == buildId);
        }
    }
}
