using System;

namespace ClientDependency.Logging
{
    public interface ILogger
    {
        void Debug(string msg);
        void Info(string msg);
        void Warn(string msg);
        void Error(string msg, Exception ex);
        void Fatal(string msg, Exception ex);
    }
}
