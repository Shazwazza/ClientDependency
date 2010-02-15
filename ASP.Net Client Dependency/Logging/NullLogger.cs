using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core.Logging
{
    internal class NullLogger : ILogger
    {
        #region ILogger Members

        public void Debug(Action action)
        {
        }

        public void Info(Action action)
        {
        }

        public void Warn(Action action)
        {
        }

        public void Error(Action action)
        {
        }

        public void Fatal(Action action)
        {
        }

        #endregion
    }
}
