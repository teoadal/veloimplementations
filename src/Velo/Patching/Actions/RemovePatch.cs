using System;
using System.Collections.Generic;

namespace Velo.Patching.Actions
{
    internal sealed class RemovePatch<T, TValue> : IPatchAction<T>
        where T : class
    {
        private readonly Func<T, ICollection<TValue>> _getter;
        private readonly TValue _value;

        public RemovePatch(Func<T, ICollection<TValue>> getter, TValue value)
        {
            _getter = getter;
            _value = value;
        }

        public void Apply(T instance)
        {
            var collection = _getter(instance);
            collection?.Remove(_value);
        }
    }
}