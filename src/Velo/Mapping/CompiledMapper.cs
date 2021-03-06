using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Velo.Utils;

namespace Velo.Mapping
{
    internal sealed class CompiledMapper<TOut> : IMapper<TOut>
    {
        private readonly Dictionary<Type, Func<object, TOut>> _converters;
        private readonly ConstructorInfo _outConstructor;
        private readonly Dictionary<string, PropertyInfo> _outProperties;
        private readonly Type _outType;

        public CompiledMapper()
        {
            _converters = new Dictionary<Type, Func<object, TOut>>();

            _outType = typeof(TOut);
            _outProperties = _outType.GetProperties().ToDictionary(p => p.Name);
            _outConstructor = ReflectionUtils.GetEmptyConstructor(_outType) 
                              ?? throw Error.DefaultConstructorNotFound(_outType);

            if (_outConstructor == null) throw Error.DefaultConstructorNotFound(_outType);
        }

        public TOut Map(object source)
        {
            var sourceType = source.GetType();
            if (_converters.TryGetValue(sourceType, out var existsConverter))
            {
                return existsConverter(source);
            }

            var converter = BuildConverter(sourceType);
            _converters.Add(sourceType, converter);

            return converter(source);
        }

        private Func<object, TOut> BuildConverter(Type sourceType)
        {
            var parameter = Expression.Parameter(typeof(object), "source");

            var sourceInstance = Expression.Variable(sourceType, "typedSource");
            var outInstance = Expression.Variable(_outType, "outInstance");

            var expressions = new List<Expression>
            {
                Expression.Assign(sourceInstance, Expression.Convert(parameter, sourceType)),
                Expression.Assign(outInstance, Expression.New(_outConstructor))
            };

            var sourceProperties = sourceType.GetProperties();
            foreach (var sourceProperty in sourceProperties)
            {
                if (!_outProperties.TryGetValue(sourceProperty.Name, out var outProperty)) continue;

                var sourceValue = Expression.Property(sourceInstance, sourceProperty);
                var outValue = Expression.Property(outInstance, outProperty);

                expressions.Add(Expression.Assign(outValue, sourceValue));
            }

            expressions.Add(outInstance); // return

            var body = Expression.Block(new[] {sourceInstance, outInstance}, expressions);
            return Expression.Lambda<Func<object, TOut>>(body, parameter).Compile();
        }
    }
}