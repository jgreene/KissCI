using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KissCI.NHibernate.Internal.Mappings;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using System.IO;

namespace KissCI.NHibernate.Internal
{
    public static class SessionManager
    {
        const string fileName = "KissCI.db3";

        static ISessionFactory _factory;
        public static ISessionFactory SessionFactory
        {
            get { 
                if(_factory != null)
                    return _factory;

                _factory = GetConfig().BuildSessionFactory();

                return _factory;
            }
        }

        static Configuration _config;
        static Configuration GetConfig()
        {
            if (_config != null)
                return _config;

            _config = Fluently.Configure()
                        .Database(
                            SQLiteConfiguration.Standard.ConnectionString("Data Source=" + fileName)
                        )
                        .Mappings(m =>
                        {
                            m.FluentMappings.Add<ProjectInfoMap>();
                            m.FluentMappings.Add<ProjectBuildMap>();
                            m.FluentMappings.Add<TaskMessageMap>();
                        }).BuildConfiguration();

            return _config;
        }

        public static void InitDb()
        {
            File.Delete(fileName);
            new SchemaUpdate(GetConfig()).Execute(true, true);
        }

        public static ISession GetSession()
        {
            var sess = SessionFactory.OpenSession();
            sess.FlushMode = FlushMode.Commit;
            sess.BeginTransaction();

            return sess;
        }
    }
}
