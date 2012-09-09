using KissCI.Helpers;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Internal
{
    public interface ILogger
    {
        void Log(string format, params object[] parameters);
    }

    namespace Loggers
    {
        public class ConsoleLogger : ILogger
        {
            public void Log(string format, params object[] parameters)
            {
                var dateTime = TimeHelper.Now;
                Console.WriteLine(dateTime.ToString() + " : " + string.Format(format, parameters));
            }
        }

        public class BuildLogger : ILogger, IDisposable
        {
            public BuildLogger(ProjectBuild build)
            {
                _build = build;
                _writer = File.AppendText(_build.LogFile);
            }

            readonly ProjectBuild _build;
            readonly StreamWriter _writer;

            public void Log(string format, params object[] parameters)
            {
                _writer.WriteLine(string.Format(format, parameters));
            }

            public void Dispose()
            {
                _writer.Dispose();
            }
        }
    }
}
