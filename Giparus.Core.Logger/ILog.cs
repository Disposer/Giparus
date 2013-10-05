namespace Giparus.Core.Logger
{
    public interface ILog
    {
        void Initialize();
        void Log(string message, int code = 0, LogType type = LogType.Inforamtion);
    }
}