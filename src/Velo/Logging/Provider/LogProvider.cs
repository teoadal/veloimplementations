using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Velo.Logging.Enrichers;
using Velo.Logging.Formatters;
using Velo.Logging.Renderers;
using Velo.Logging.Writers;
using Velo.Serialization.Models;
using Velo.Utils;

namespace Velo.Logging.Provider
{
    internal sealed class LogProvider : ILogProvider
    {
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        public LogLevel Level => _minimalLevel;
        
        private readonly ILogEnricher[] _enrichers;
        private readonly IRenderersCollection _renderers;
        private readonly LogLevel _minimalLevel;
        private readonly ILogWriter[] _writers;

        public LogProvider(ILogEnricher[] enrichers, IRenderersCollection renderers, ILogWriter[] writers)
        {
            _enrichers = enrichers;
            _renderers = renderers;
            _writers = writers;

            _minimalLevel = writers.Min(w => w.Level);
        }

        public void Write(LogLevel level, Type sender, string message)
        {
            if (level < _minimalLevel) return;
            if (string.IsNullOrWhiteSpace(message)) throw Error.Null(nameof(message));

            var buffer = Renderer.GetBuffer(_enrichers.Length);

            Enrich(level, sender, buffer);

            WriteMessage(level, sender, null, message, buffer);
            Renderer.ReturnBuffer(buffer);
        }

        public void Write<T1>(LogLevel level, Type sender, string template, T1 arg1)
        {
            if (level < _minimalLevel) return;

            var buffer = Renderer.GetBuffer(_enrichers.Length + 1);

            Enrich(level, sender, buffer);

            var renderer = _renderers.GetRenderer<Renderer<T1>>(template);
            renderer.Render(buffer, arg1);

            WriteMessage(level, sender, renderer.Formatter, template, buffer);
        }

        public void Write<T1, T2>(LogLevel level, Type sender, string template, T1 arg1, T2 arg2)
        {
            if (level < _minimalLevel) return;

            var buffer = Renderer.GetBuffer(_enrichers.Length + 2);

            Enrich(level, sender, buffer);

            var renderer = _renderers.GetRenderer<Renderer<T1, T2>>(template);
            renderer.Render(buffer, arg1, arg2);

            WriteMessage(level, sender, renderer.Formatter, template, buffer);
        }

        public void Write<T1, T2, T3>(LogLevel level, Type sender, string template, T1 arg1, T2 arg2, T3 arg3)
        {
            if (level < _minimalLevel) return;

            var buffer = Renderer.GetBuffer(_enrichers.Length + 3);

            Enrich(level, sender, buffer);

            var renderer = _renderers.GetRenderer<Renderer<T1, T2, T3>>(template);
            renderer.Render(buffer, arg1, arg2, arg3);

            WriteMessage(level, sender, renderer.Formatter, template, buffer);
        }

        public void Write(LogLevel level, Type sender, string template, params object[] args)
        {
            if (level < _minimalLevel) return;

            var buffer = Renderer.GetBuffer(_enrichers.Length + args.Length);

            Enrich(level, sender, buffer);

            var arrayRenderer = _renderers.GetArrayRenderer(template, args);
            arrayRenderer.Render(buffer, args);

            WriteMessage(level, sender, arrayRenderer.Formatter, template, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Enrich(LogLevel level, Type sender, JsonObject message)
        {
            foreach (var enricher in _enrichers)
            {
                enricher.Enrich(level, sender, message);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteMessage(LogLevel level, Type sender, ILogFormatter? formatter, string template, JsonObject buffer)
        {
            var context = new LogContext(level, sender, template, formatter);

            foreach (var writer in _writers)
            {
                if (writer.Level > level) continue;
                writer.Write(context, buffer);
            }

            Renderer.ReturnBuffer(buffer);
        }
    }
}