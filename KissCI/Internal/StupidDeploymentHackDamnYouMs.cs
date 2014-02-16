using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KissCI.Internal
{
    public static class StupidDeploymentHackDamnYouMs
    {
        static StupidDeploymentHackDamnYouMs()
        {
            try
            {
                throw new System.Data.SQLite.SQLiteException("This class should never be called or instantiated");
            }
            catch { }
        }
    }
}
