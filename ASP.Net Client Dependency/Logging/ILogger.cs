using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration.Provider;

namespace ClientDependency.Core.Logging
{
    public interface ILogger
    {
        void Debug(Action action);
        void Info(Action action);
        void Warn(Action action);
        void Error(Action action);
        void Fatal(Action action);
    }
}
