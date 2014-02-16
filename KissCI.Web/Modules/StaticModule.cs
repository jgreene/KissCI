using KissCI.Web.Internal;
using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Web.Modules
{
    public class StaticModule : NancyModule
    {
        public StaticModule() : base("/content")
        {
            Get["/{path*}"] = (ctx) => {
                var resourceNamespace = "KissCI.Web.Content";
                string path = ctx.path;
                if(string.IsNullOrEmpty(path))
                    return null;

                return new EmbeddedFileResponse
                            (
                                typeof(StaticModule).Assembly
                                , resourceNamespace
                                , path
                            );
            };
        }
    }
}
