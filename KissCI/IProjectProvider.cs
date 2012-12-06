using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI
{
    public interface IConfiguration
    {
        string Get(string key);
        T Get<T>(string key);
    }

    public interface IProjectProvider
    {
        IEnumerable<Project> Projects(IConfiguration config);
    }
}
