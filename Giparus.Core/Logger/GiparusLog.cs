using System.Diagnostics;

namespace Giparus.Core.Logger
{
    public static class GiparusLog
    {
        #region Fields
        public const int Listener = 0;
        public const int Manager = 1;
        public const int Scheduler = 2;
        public const int DataConnector = 3;

        public const ushort CODE_GENERIC_ERROR = 0xff00;
        public const ushort CODE_INFORMATION = 0x0;
        public const ushort CODE_GENERIC_WARNING = 0x00ff;
        public const ushort CODE_SUCCESS = 0x0;

        private const string LOG_FORMAT = "{0}, Reason:{1}";

        private static ILog _logProvider;
        private static ILog _eventView;
        private static bool _initialized;

        public static bool LogToEventViewer { get; set; } 
        #endregion

        
        #region .ctor & Initializers
        public static void Initialize(ILog logProvider)
        {
            if (_initialized) return;

            _logProvider = logProvider;
            _eventView = new EventViewerLogProvider();

            _logProvider.Initialize();
            _eventView.Initialize();

            LogToEventViewer = true;
            _initialized = true;
        }

        static GiparusLog()
        {
            var logProvider = new ConsoleLogProvider();
            Initialize(logProvider);
        }
        #endregion


        [DebuggerNonUserCode]
        public static void ReplaceProvider(ILog logProvider)
        {
            logProvider.Initialize();
            _logProvider = logProvider;
        }

        [DebuggerNonUserCode]
        public static void Initialize()
        {
            Initialize(new ConsoleLogProvider());
        }

        [DebuggerNonUserCode]
        public static void Log(string message, int code = 0, LogType type = LogType.Inforamtion, bool logAsEvent = false)
        {
            if (_logProvider == null) return;

            _logProvider.Log(message, code, type);
            if (logAsEvent && LogToEventViewer) _eventView.Log(message, code, type);
        }

        [DebuggerNonUserCode]
        public static void LogEvent(string message, int code = 0, LogType type = LogType.Inforamtion)
        {
            if (LogToEventViewer) _eventView.Log(message, code, type);
        }

        [DebuggerNonUserCode]
        public static void LogError(string message, string reason, int code = CODE_GENERIC_ERROR, bool logAsEvent = false)
        {
            var text = string.Format(LOG_FORMAT, message, reason);
            Log(text, code, LogType.Failure, logAsEvent);
        }

        [DebuggerNonUserCode]
        public static void LogWarning(string message, string reason, int code = CODE_GENERIC_WARNING, bool logAsEvent = false)
        {
            var text = string.Format(LOG_FORMAT, message, reason);
            Log(text, code, LogType.Warning, logAsEvent);
        }

        [DebuggerNonUserCode]
        public static void SetLogProvider(ILog provider)
        {
            _logProvider = provider;
            _logProvider.Initialize();
        }
    }
}
