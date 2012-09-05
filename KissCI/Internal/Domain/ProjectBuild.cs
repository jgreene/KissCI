using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KissCI.Internal.Domain
{
    public enum BuildResult : long
    {
        None,
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
        Starting,
        Building,
        CleaningUp
    }


    public class TaskMessage
    {
        public virtual long Id { get; set; }
        public virtual long BuildId { get; set; }
        public virtual DateTime Time { get; set; }
        public virtual string Message { get; set; }
    }

    public class ProjectBuild
    {
        public virtual long Id { get; set; }
        public virtual long ProjectInfoId { get; set; }
        public virtual DateTime BuildTime { get; set; }
        public virtual DateTime CompleteTime { get; set; }
        public virtual Nullable<BuildResult> BuildResult { get; set; }
        public virtual Lazy<Stream> BuildLog { get; set; }
    }

    public class ProjectInfo
    {
        public virtual long Id { get; set; }
        public virtual string ProjectName { get; set; }
        public virtual Status Status { get; set; }
        public virtual Activity Activity { get; set; }

        #region calculated
        public virtual Nullable<BuildResult> LastBuildResult { get; set; }
        public virtual Nullable<DateTime> LastBuildTime { get; set; }
        public virtual Nullable<DateTime> NextBuildTime { get; set; }
        public virtual string LastMessage { get; set; }
        #endregion
    }
}
