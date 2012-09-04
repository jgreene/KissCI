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
        public long Id { get; set; }
        public long BuildId { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }

    public class ProjectBuild
    {
        public long Id { get; set; }
        public long ProjectInfoId { get; set; }
        public DateTime BuildTime { get; set; }
        public DateTime CompleteTime { get; set; }
        public Nullable<BuildResult> BuildResult { get; set; }
        public Lazy<Stream> BuildLog { get; set; }
    }

    public class ProjectInfo
    {
        public long Id { get; set; }
        public string ProjectName { get; set; }
        public Status Status { get; set; }
        public Activity Activity { get; set; }

        #region calculated
        public Nullable<BuildResult> LastBuildResult { get; set; }
        public Nullable<DateTime> LastBuildTime { get; set; }
        public Nullable<DateTime> NextBuildTime { get; set; }
        public string LastMessage { get; set; }
        #endregion
    }
}
