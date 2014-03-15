namespace KissCI.Web
{
    using System;
    using Nancy.Hosting.Self;
    using NDesk.Options;

    class Program
    {
        static void Main(string[] args)
        {
            string hostUri = "";
            bool showHelp = false;

            var options = new OptionSet(){
                { "help", "show this message and exit", v => showHelp = v != null },
                { "h|host=", "the {HOST} to run on", v => hostUri = v }
            };

            try
            {
                options.Parse(args);
            }
            catch(OptionException ex)
            {   
                Console.Write("KissCI.Web: ");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try `KissCI.Web.exe --help' for more information.");
                return;
            }

            var url = !string.IsNullOrEmpty(hostUri) ? hostUri : "http://localhost:3579";

            var uri = new Uri(url);
            try
            {
                using (var host = new NancyHost(uri))
                {
                    host.Start();

                    Console.WriteLine("Your application is running on " + uri);
                    Console.WriteLine("Press any [Enter] to close the host.");
                    Console.ReadLine();
                }
            }
            catch
            {
                Console.WriteLine("This program must be run as an administrator.");
                throw;
            }

            
        }
    }
}
