using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public enum BuildResult : long
    {
        Cancelled,
        Failure,
        Success
    }

    public enum Status : long
    {
        Running,
        Stopped
    }

    public enum Activity : long
    {
        Sleeping,
        Building,
        CleaningUp
    }

    public enum MessageType : long
    {
        LogMessage,
        TaskMessage
    }

    public class TaskMessage
    {
        

        public virtual long Id { get; set; }
        public virtual long ProjectBuildId { get; set; }
        public virtual long ProjectInfoId { get; set; }
        public virtual DateTime Time { get; set; }
        public virtual string Message { get; set; }
        public virtual MessageType Type { get; set; }
    }

    public class ProjectBuild
    {
        public virtual long Id { get; set; }
        public virtual long ProjectInfoId { get; set; }
        public virtual DateTime BuildTime { get; set; }
        public virtual DateTime? CompleteTime { get; set; }
        public virtual Nullable<BuildResult> BuildResult { get; set; }
    }

    public class ProjectInfo
    {
        public virtual long Id { get; set; }
        public virtual string ProjectName { get; set; }
        public virtual string Category { get; set; }
        public virtual Status Status { get; set; }
        public virtual Activity Activity { get; set; }
    }

    public class ProjectView
    {
        public ProjectInfo Info { get; set; }

        public ProjectBuild LastBuild { get; set; }
        public TaskMessage LastMessage { get; set; }

        public virtual Nullable<DateTime> NextBuildTime { get; set; }
    }
}
