using KissCI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI
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

        public class FileLogger : ILogger
        {
            public FileLogger(string directory)
            {
                _directory = directory;
            }

            string _directory;

            public void Log(string format, params object[] parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}
