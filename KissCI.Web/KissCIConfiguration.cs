using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Web
{
    

    public class KissCIConfiguration
    {
        public KissCIConfiguration()
        {
            this.Path = "/KissCI";
            this.ProjectDirectory = "Projects";
        }

        string _path;
        public string Path
        {
            get {  return _path; }
            set { _path = (!value.StartsWith("/")) ? string.Concat("/", value) : value; }
        }

        public string ProjectDirectory { get; set; }
    }

    
}
