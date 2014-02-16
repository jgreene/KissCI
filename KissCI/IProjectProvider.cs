using KissCI.Helpers;
using KissCI.Internal;
using KissCI.Triggers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace KissCI
{

    public interface IProjectProvider : IHideObjectMembers
    {
        IEnumerable<KissProject> Projects();
    }

    public abstract class ProjectBuilder : IHideObjectMembers { }

    public static class ProjectProviderExtensions
    {

        class KissProjectBuilder : ProjectBuilder
        {
            public string ProjectName { get; set; }
            public string Category { get; set; }

            List<KissCommand> _commands = new List<KissCommand>();

            public List<KissCommand> Commands { get { return _commands; } }
        }

        public static ProjectBuilder CreateProject(this IProjectProvider provider, string projectName, string category)
        {
            if(string.IsNullOrEmpty(projectName))
                throw new ArgumentNullException("projectName");

            if(string.IsNullOrEmpty(category))
                throw new ArgumentNullException("category");

            return new KissProjectBuilder { ProjectName = projectName, Category = category };
        }

        public static ProjectBuilder WithCommand<TResult>(
            this ProjectBuilder projectBuilder, 
            string commandName,
            Func<KissTask<KissCITaskStart, KissCITaskStart>, KissTask<KissCITaskStart, TResult>> act,
            params ITrigger[] triggers
        )
        {
            var tasks = act(TaskHelper.Start()).Finalize();

            var kissBuilder = (KissProjectBuilder)projectBuilder;
            kissBuilder.Commands.Add(new KissCommand(commandName, tasks, triggers));
            
            return kissBuilder;
        }

        public static KissProject ToKissProject(ProjectBuilder projectBuilder)
        {
            var kissBuilder = (KissProjectBuilder)projectBuilder;

            return new KissProject(kissBuilder.ProjectName, kissBuilder.Category, kissBuilder.Commands.ToArray());
        }
    }
}
