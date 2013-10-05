using System.Diagnostics;

namespace Giparus.Core.Logger
{
    class EventViewerLogProvider : ILog
    {
        private const string EVENT_SOURCE = "Elemon Logs";
        private const string EVENT_REPORTING = "Elemon";

        public void Initialize()
        {
            if (EventLog.SourceExists(EVENT_SOURCE)) return;
            EventLog.CreateEventSource(EVENT_SOURCE, EVENT_REPORTING);
            EventLog.WriteEntry(EVENT_SOURCE, "Elemon log Created", EventLogEntryType.Information);
        }

        public void Log(string message, int code, LogType type = LogType.Inforamtion)
        {
            EventLog.WriteEntry(EVENT_SOURCE, message, (EventLogEntryType)type, code);
        }
    }
}
