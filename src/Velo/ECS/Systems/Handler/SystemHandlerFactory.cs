using System;
using Velo.DependencyInjection;
using Velo.DependencyInjection.Dependencies;
using Velo.DependencyInjection.Factories;
using Velo.DependencyInjection.Resolvers;
using Velo.Threading;

namespace Velo.ECS.Systems.Handler
{
    internal sealed class SystemHandlerFactory : IDependencyFactory
    {
        private readonly Type _contract;

        public SystemHandlerFactory()
        {
            _contract = typeof(ISystemHandler<>);
        }

        public bool Applicable(Type contract)
        {
            return contract.IsGenericType && contract.GetGenericTypeDefinition() == _contract;
        }

        public IDependency BuildDependency(Type contract, IDependencyEngine engine)
        {
            var systemType = contract.GenericTypeArguments[0];
            var dependencies = engine.GetApplicable(systemType);

            var contracts = new[] {contract};

            if (dependencies.Length == 0)
            {
                var nullHandlerType = typeof(SystemNullHandler<>).MakeGenericType(systemType);
                var nullHandler = Activator.CreateInstance(nullHandlerType);
                return new InstanceDependency(contracts, nullHandler);
            }

            var handlerType = DefineHandlerType(dependencies);

            var implementation = handlerType.MakeGenericType(systemType);
            var lifetime = dependencies.DefineLifetime();
            var resolver = DependencyResolver.Build(lifetime, implementation, engine);

            return Dependency.Build(lifetime, contracts, resolver);
        }

        private static Type DefineHandlerType(IDependency[] dependencies)
        {
            if (dependencies.Length == 1)
            {
                return typeof(SystemSingleHandler<>);
            }

            bool hasParallels = false, hasSequential = false;

            foreach (var dependency in dependencies)
            {
                if (ParallelAttribute.IsDefined(dependency.Resolver.Implementation))
                {
                    hasParallels = true;
                }
                else
                {
                    hasSequential = true;
                }
            }

            return hasParallels
                ? hasSequential
                    ? typeof(SystemFullHandler<>)
                    : typeof(SystemParallelHandler<>)
                : typeof(SystemSequentialHandler<>);
        }
    }
}