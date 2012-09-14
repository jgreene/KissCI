using KissCI.Internal;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Helpers
{
    public static class ServiceHelper
    {
        public static IProjectService GetService(string root)
        {
            return new MainProjectService(root);
        }
    }
}
