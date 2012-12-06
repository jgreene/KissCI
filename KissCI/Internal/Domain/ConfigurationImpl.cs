using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public class ConfigurationImpl : IConfiguration
    {
        public ConfigurationImpl(IDataContext context)
        {
            _context = context;
        }

        readonly IDataContext _context;

        public string Get(string key)
        {
            return _context.ConfigurationService.Get(key);
        }

        public T Get<T>(string key)
        {
            Type typ = typeof(T);
            var converter = TypeDescriptor.GetConverter(typ);

            if (converter == null)
                return default(T);

            if (converter.CanConvertFrom(typeof(string)) == false)
                return default(T);

            string value = Get(key);

            return (T)converter.ConvertFromString(value);
        }

        
    }
}
