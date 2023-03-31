using System;

namespace Hci.WebsiteDolly.Core.Domain
{
    public class ProcessorResult
    {
        public ProcessorResultStatus Status
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public ProcessorResult(ProcessorResultStatus status, int count, string message, Exception exception)
        {
            Exception = exception;
            Message = message;
            Status = status;
            Count = count;
        }

        public ProcessorResult(Exception exception, string message)
            : this(ProcessorResultStatus.Exception, 0, message, exception) { }

        public ProcessorResult(ProcessorResultStatus status, int count, string message)
            : this(status, count, message, null) { }

        public ProcessorResult()
            : this(ProcessorResultStatus.NotSet, 0, string.Empty) { }
    }
}
