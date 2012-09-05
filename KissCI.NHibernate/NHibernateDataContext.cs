using KissCI.NHibernate.Internal;
using KissCI.Internal;
using KissCI.Internal.Domain;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace KissCI.NHibernate
{
    public class NHibernateDataContext : IDataContext
    {
        public NHibernateDataContext()
        {
            _session = SessionManager.GetSession();
        }

        readonly ISession _session;

        public ITaskMessageService TaskMessageService
        {
            get { return new TaskMessageService(_session); }
        }

        public IProjectBuildService ProjectBuildService
        {
            get { return new ProjectBuildService(_session); }
        }

        public IProjectInfoService ProjectInfoService
        {
            get { return new ProjectInfoService(_session); }
        }

        public void Commit()
        {
            if (_session.Transaction.IsActive
                && _session.Transaction.WasRolledBack == false
                && _session.IsConnected
                && _session.IsOpen
                )
            {
                _session.Transaction.Commit();
            }
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
