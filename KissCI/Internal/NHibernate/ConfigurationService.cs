using KissCI.Internal.Domain;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;

namespace KissCI.Internal.NHibernate
{
    public class ConfigurationService : IConfigurationService
    {
        public ConfigurationService(ISession session)
        {
            _session = session;
        }

        readonly ISession _session;

        public string Get(string key)
        {
            var item = _session.Query<ConfigurationItem>().FirstOrDefault(i => i.Key == key);

            return item == null ? null : item.Value;
        }


        public void Save(string key, string value)
        {
            var item = _session.Query<ConfigurationItem>().FirstOrDefault(i => i.Key == key) ?? new ConfigurationItem { Key = key };

            item.Value = value;

            _session.SaveOrUpdate(item);
        }

        public void Remove(string key)
        {
            var item = _session.Query<ConfigurationItem>().FirstOrDefault(i => i.Key == key);

            if (item == null)
                return;

            _session.Delete(item);
        }

        public IEnumerable<KeyValuePair<string, string>> All()
        {
            return _session.Query<ConfigurationItem>().Select(i => new KeyValuePair<string, string>(i.Key, i.Value)).ToList();
        }
    }
}
