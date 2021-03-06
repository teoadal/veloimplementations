using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Velo.Collections.Enumerators;
using Velo.ECS.Components;

namespace Velo.ECS.Assets.Filters
{
    public interface IAssetFilter<TComponent1, TComponent2> : IAssetFilter, IEnumerable<Asset<TComponent1, TComponent2>>
        where TComponent1 : IComponent where TComponent2 : IComponent
    {
        bool TryGet(int assetId, out Asset<TComponent1, TComponent2> asset);

        IEnumerable<Asset<TComponent1, TComponent2>> Where<TArg>(
            Func<Asset<TComponent1, TComponent2>, TArg, bool> filter, TArg arg);
    }

    [DebuggerTypeProxy(typeof(DebuggerVisualizer<,>))]
    [DebuggerDisplay("Length = {" + nameof(Length) + "}")]
    internal sealed class AssetFilter<TComponent1, TComponent2> : IAssetFilter<TComponent1, TComponent2>
        where TComponent1 : IComponent where TComponent2 : IComponent
    {
        public int Length => _assets.Length;

        private readonly Asset<TComponent1, TComponent2>[] _assets;

        public AssetFilter(Asset[] assets)
        {
            var buffer = new List<Asset<TComponent1, TComponent2>>();
            foreach (var asset in assets)
            {
                if (asset.TryGetComponents<TComponent1, TComponent2>(out var component1, out var component2))
                {
                    buffer.Add(new Asset<TComponent1, TComponent2>(asset, component1, component2));
                }
            }

            _assets = buffer.ToArray();
        }

        public bool Contains(int assetId) => TryGet(assetId, out _);

        public IEnumerator<Asset<TComponent1, TComponent2>> GetEnumerator()
        {
            return new ArrayEnumerator<Asset<TComponent1, TComponent2>>(_assets);
        }

        public bool TryGet(int assetId, out Asset<TComponent1, TComponent2> asset)
        {
            foreach (var exists in _assets)
            {
                if (exists.Entity.Id != assetId) continue;

                asset = exists;
                return true;
            }

            asset = default;
            return false;
        }

        public IEnumerable<Asset<TComponent1, TComponent2>> Where<TArg>(
            Func<Asset<TComponent1, TComponent2>, TArg, bool> filter, TArg arg)
        {
            return new ArrayWhereEnumerator<Asset<TComponent1, TComponent2>, TArg>(_assets, filter, arg);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}