using System.Web;
using System.Web.Optimization;

namespace KissCI.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css")
                .Include("~/Content/css/bootstrap.css")
                .Include("~/Content/site.css")
            );


            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js")
                        
            );

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*")
            );

            bundles.Add(new ScriptBundle("~/bundles/bootstrap")
                .Include("~/Content/js/bootstrap.js")
            );

            bundles.Add(new ScriptBundle("~/bundles/project")
                .Include("~/Scripts/underscore.js")
                .Include("~/Scripts/angular.js")
                .Include("~/Scripts/projectviewtable.js")
                .Include("~/Scripts/configuration.js")
                
            );

            
        }
    }
}