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

    public abstract class KissProjectBuilder : IHideObjectMembers { }

    public static class ProjectProviderExtensions
    {

        class InternalKissProjectBuilder : KissProjectBuilder
        {
            public string ProjectName { get; set; }
            public string Category { get; set; }

            List<KissCommand> _commands = new List<KissCommand>();

            public List<KissCommand> Commands { get { return _commands; } }
        }

        public static KissProjectBuilder CreateProject(this IProjectProvider provider, string projectName, string category)
        {
            if(string.IsNullOrEmpty(projectName))
                throw new ArgumentNullException("projectName");

            if(string.IsNullOrEmpty(category))
                throw new ArgumentNullException("category");

            return new InternalKissProjectBuilder { ProjectName = projectName, Category = category };
        }

        public static KissProjectBuilder WithCommand<TResult>(
            this KissProjectBuilder projectBuilder, 
            string commandName,
            Func<KissTask<KissCITaskStart, KissCITaskStart>, KissTask<KissCITaskStart, TResult>> act,
            params ITrigger[] triggers
        )
        {
            var tasks = act(TaskHelper.Start()).Finalize();

            var kissBuilder = (InternalKissProjectBuilder)projectBuilder;
            kissBuilder.Commands.Add(new KissCommand(commandName, tasks, triggers));
            
            return kissBuilder;
        }

        public static KissProject ToKissProject(KissProjectBuilder projectBuilder)
        {
            var kissBuilder = (InternalKissProjectBuilder)projectBuilder;

            return new KissProject(kissBuilder.ProjectName, kissBuilder.Category, kissBuilder.Commands.ToArray());
        }
    }
}
