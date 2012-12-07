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
        static readonly object configLock = new object();
        static readonly object factoryLock = new object();

        static IDictionary<string, Configuration> _configs = new Dictionary<string, Configuration>();
        static IDictionary<string, ISessionFactory> _factories = new Dictionary<string, ISessionFactory>();

        static string GetDbFile(string root)
        {
            return Path.Combine(root, "KissCI.db3");
        }

        static string GetConnectionString(string root)
        {
            return string.Format("Data Source={0};Version=3;New=True;FailIfMissing=False", GetDbFile(root));
        }

        static ISessionFactory GetFactory(string root)
        {
            lock (factoryLock)
            {
                if (_factories.ContainsKey(root))
                    return _factories[root];

                var config = GetConfig(root);

                var fact = config.BuildSessionFactory();

                _factories.Add(root, fact);

                return fact;
            }
        }

        static Configuration GetConfig(string root)
        {
            lock (configLock)
            {
                if (_configs.ContainsKey(root))
                    return _configs[root];

                var connectionString = GetConnectionString(root);

                var config = Fluently.Configure()
                            .Database(
                                SQLiteConfiguration.Standard.ConnectionString(connectionString)
                            )
                            .Mappings(m =>
                            {
                                m.FluentMappings.Add<ProjectInfoMap>();
                                m.FluentMappings.Add<ProjectBuildMap>();
                                m.FluentMappings.Add<TaskMessageMap>();
                                m.FluentMappings.Add<ConfigurationItemMap>();
                            })
                            .ExposeConfiguration(cfg =>
                            {
                                var filePath = GetDbFile(root);
                                if (File.Exists(filePath) == false)
                                {
                                    using (var file = File.Create(filePath)) { }
                                    new SchemaUpdate(cfg).Execute(false, true);
                                    //new SchemaExport(cfg).Execute(false, true, true);
                                }
                                else
                                {
                                    new SchemaUpdate(cfg).Execute(false, true);
                                }

                            })
                            .BuildConfiguration();

                _configs.Add(root, config);

                return config;
            }
        }

        public static void Clear()
        {
            lock (configLock)
            {
                _configs.Clear();
            }

            lock (factoryLock)
            {
                _factories.Clear();
            }
        }

        public static ISession GetSession(string root)
        {
            var factory = GetFactory(root);

            var sess = factory.OpenSession();
            sess.FlushMode = FlushMode.Commit;
            sess.BeginTransaction();

            return sess;
        }
    }
}
