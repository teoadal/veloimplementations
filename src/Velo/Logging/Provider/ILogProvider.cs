using System;

namespace Velo.Logging.Provider
{
    internal interface ILogProvider
    {
        void Write(LogLevel level, Type sender, string template);

        void Write<T1>(LogLevel level, Type sender, string template, T1 arg1);

        void Write<T1, T2>(LogLevel level, Type sender, string template, T1 arg1, T2 arg2);

        void Write<T1, T2, T3>(LogLevel level, Type sender, string template, T1 arg1, T2 arg2, T3 arg3);

        void Write(LogLevel level, Type sender, string template, params object[] args);
    }
}