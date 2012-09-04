using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Helpers
{
    public static class ProcessHelper
    {
        public static Process Start(string processName, string args)
        {
            var startInfo = new ProcessStartInfo(processName, args);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            return Process.Start(startInfo);
        }
    }
}
