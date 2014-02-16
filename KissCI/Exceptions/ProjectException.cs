using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Exceptions
{
    public class ProjectException : Exception
    {
        public ProjectException(string message) : base(message) { }
    }
}
