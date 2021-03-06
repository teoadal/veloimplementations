namespace Velo.Logging
{
    // ReSharper disable once UnusedTypeParameter
    public interface ILogger<TSender>
    {
        void Log(LogLevel level, string message);

        void Log<T1>(LogLevel level, string template, T1 arg1);

        void Log<T1, T2>(LogLevel level, string template, T1 arg1, T2 arg2);

        void Log<T1, T2, T3>(LogLevel level, string template, T1 arg1, T2 arg2, T3 arg3);

        void Log(LogLevel level, string template, params object[] args);

        #region Trace

        void Trace(string message);

        void Trace<T1>(string template, T1 arg1);

        void Trace<T1, T2>(string template, T1 arg1, T2 arg2);

        void Trace<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3);

        void Trace(string template, params object[] args);

        #endregion

        #region Debug

        void Debug(string message);

        void Debug<T1>(string template, T1 arg1);

        void Debug<T1, T2>(string template, T1 arg1, T2 arg2);

        void Debug<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3);

        void Debug(string template, params object[] args);

        #endregion

        #region Info

        void Info(string message);

        void Info<T1>(string template, T1 arg1);

        void Info<T1, T2>(string template, T1 arg1, T2 arg2);

        void Info<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3);

        void Info(string template, params object[] args);

        #endregion

        #region Warning

        void Warning(string message);

        void Warning<T1>(string template, T1 arg1);

        void Warning<T1, T2>(string template, T1 arg1, T2 arg2);

        void Warning<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3);

        void Warning(string template, params object[] args);

        #endregion

        #region Error

        void Error(string message);

        void Error<T1>(string template, T1 arg1);

        void Error<T1, T2>(string template, T1 arg1, T2 arg2);

        void Error<T1, T2, T3>(string template, T1 arg1, T2 arg2, T3 arg3);

        void Error(string template, params object[] args);

        #endregion
    }
}