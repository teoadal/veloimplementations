using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Velo.Serialization.Models;
using Velo.Utils;

namespace Velo.ECS.Sources.Json.References
{
    internal sealed class ListReferenceResolver<TOwner, TEntity> : ReferenceResolver<TOwner, TEntity>
        where TOwner : class where TEntity : class, IEntity
    {
        private readonly Func<TOwner, IList<TEntity?>> _entityGetter;
        private readonly Action<TOwner, List<TEntity?>> _entitySetter;

        private readonly string _propertyName;

        public ListReferenceResolver(PropertyInfo property, Func<int, TEntity> resolver)
            : base(resolver)
        {
            _propertyName = property.Name;

            var owner = Typeof<TOwner>.Raw;
            _entityGetter = (Func<TOwner, IList<TEntity?>>) ExpressionUtils.BuildGetter(owner, property);
            _entitySetter = (Action<TOwner, List<TEntity?>>) ExpressionUtils.BuildSetter(owner, property);
        }

        public override void Read(JsonObject source, TOwner instance)
        {
            var ids = (JsonArray) source[_propertyName];

            var entities = new List<TEntity?>(ids.Length);

            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var id in ids)
            {
                var entity = ReadEntity(id);
                entities.Add(entity);
            }

            _entitySetter(instance, entities);
        }

        public override void Serialize(TOwner instance, TextWriter output)
        {
            var entities = _entityGetter(instance);

            output.Write('[');

            var first = true;
            foreach (var entity in entities)
            {
                if (first) first = false;
                else output.Write(',');

                SerializeEntity(entity, output);
            }

            output.Write(']');
        }
    }
}