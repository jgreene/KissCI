using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public interface IConfigurationService
    {
        string Get(string key);
        void Save(string key, string value);
        void Remove(string key);

        IEnumerable<KeyValuePair<string, string>> All();
    }
}
