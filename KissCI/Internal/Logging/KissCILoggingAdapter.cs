using Common.Logging;
using Common.Logging.Factory;
using KissCI.Helpers;
using KissCI.Internal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KissCI.Internal.Logging
{
    public class KissCILoggingAdapter : AbstractLogger
    {
        readonly IProjectService _projectService;
        readonly ProjectInfo _info;
        readonly ProjectBuild _build;
        public KissCILoggingAdapter(IProjectService projectService, ProjectInfo info, ProjectBuild build)
        {
            _projectService = projectService;
            _info = info;
            _build = build;
        }

        public override bool IsDebugEnabled
        {
            get { return true; }
        }

        public override bool IsErrorEnabled
        {
            get { return true; }
        }

        public override bool IsFatalEnabled
        {
            get { return true; }
        }

        public override bool IsInfoEnabled
        {
            get { return true; }
        }

        public override bool IsTraceEnabled
        {
            get { return true; }
        }

        public override bool IsWarnEnabled
        {
            get { return true; }
        }

        KissCI.Internal.Domain.LogType GetLogType(LogLevel level)
        {
            return (LogType)Convert.ToInt64((int)level);
        }

        protected override void WriteInternal(LogLevel level, object message, Exception exception)
        {
            var getLogMessage = new Func<string>(() => {

                if(exception != null && message != null)
                    return message + Environment.NewLine + exception.ToString();

                if(exception != null)
                    return exception.ToString();

                if(message != null)
                    return message.ToString();

                return "";
                
            });

            var fullMessage = getLogMessage();
            var logType = GetLogType(level);

            Console.WriteLine(fullMessage);

            using (var ctx = _projectService.OpenContext())
            {
                ctx.TaskMessageService.WriteMessage(new Internal.Domain.TaskMessage
                {
                    ProjectInfoId = _info.Id,
                    ProjectBuildId = _build.Id,
                    Time = TimeHelper.Now,
                    Message = fullMessage,
                    Type = MessageType.LogMessage,
                    LogType = logType
                });

                ctx.Commit();
            }
        }
    }
}
