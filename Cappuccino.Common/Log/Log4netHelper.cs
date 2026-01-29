using System;

namespace Cappuccino.Common.Log
{
    public static class Log4netHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("LogHelper");
        public static void Debug(string message)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message);
            }
        }
        public static void Debug(object message, Exception exception)
        {
            if (log.IsErrorEnabled)
            {
                log.Debug(message, exception);
            }
        }
        public static void Error(string message)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(message);
            }
        }
        public static void Error(object message, Exception exception)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(message, exception);
            }
        }
        public static void Fatal(string message)
        {
            if (log.IsFatalEnabled)
            {
                log.Fatal(message);
            }
        }
        public static void Fatal(object message, Exception exception)
        {
            if (log.IsFatalEnabled)
            {
                log.Fatal(message, exception);
            }
        }
        public static void Info(string message)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message);
            }
        }
        public static void Info(object message, Exception exception)
        {

            log.Info(message, exception);
        }
        public static void Warn(string message)
        {
            if (log.IsWarnEnabled)
            {
                log.Warn(message);
            }
        }
        public static void Warn(object message, Exception exception)
        {
            log.Warn(message, exception);
        }
    }
}