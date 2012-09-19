using KissCI.Tests.Projects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Internal
{
    public class StupidDeploymentHackDamnYouMs
    {
        public StupidDeploymentHackDamnYouMs()
        {
            new System.Data.SQLite.SQLiteException();
            new ProjectProvider();
            throw new Exception("This class should never be called or instantiated");
        }
    }
}
