KissCI - A simple continuous integration server
=======================================

KissCI is a simple continuous integration server built in C# with the stated goal of staying out of your way and letting you do what you already know how to do, which is program.  (If you don't know how to program this project is not for you.)


#### Getting Started

To get started make a new class library project and reference the KissCI.dll (and its dependencies).  You'll need to implement a single interface from KissCI called IProjectProvider.  Here is a simple example:

    using KissCI;
    using KissCI.Helpers;
    using KissCI.Tasks;

    public class MyProjects : IProjectProvider {

        class MyTaskResult {
            public string Property { get; set; }
        }

        public IEnumerable<Project> Projects(){

            var tasks = TaskHelper.Start()
            .AddTask("My Task", (ctx, arg) => {
                ctx.Log("My task only logs something");

                return new MyTaskResult { Property = "My task property result" };
            })
            .AddTask("My Second Task", (ctx, arg) => {
                ctx.Log("My second task logs the property of the first task: {0}", arg.Property);

                return false;
            })
            .Finalize();

            var project = new Project("My Project", "My Category", tasks);

            project.AddTimer(DateTime.Parse("09/01/2012 1:00:00 PM"));

            yield return project;
        }

    }

The only complexity above is dealing with tasks and the concept of returning a task result.  Whatever is returned in the AddTask call becomes the argument to the next AddTask call.  This allows us to create composable tasks that build off of each other.  An example of this exists in the project as .TempMsBuild4_0(rootDirectory, projectFile, outputDirectory) which creates a unique temporary folder to build the project in, transfers the result to the output directory, and then cleans up the temporary directory.

If at any point you need to massage data into a different format/class you can use the AddStep method.  This bypasses the default logging in AddTask but is otherwise the same as AddTask.

#### Setting it up

Go to https://github.com/jgreene/KissCI/downloads and download KissCI.Service.zip.  This should be the latest KissCI build.

Extract this .zip file to a directory of your choosing and then register it as a windows service by running:

    KissCI.Service.exe -install KissCIService

To uninstall call:

    KissCI.Service.exe -uninstall KissCIService

You can choose whatever name you would like for your service by simply changing "KissCIService" to whatever you would like.

#### Configuring KissCI

The configuration for KissCI lives within the KissCI.Service.exe.config file.  Currently there are only two options: Port and HostName.

