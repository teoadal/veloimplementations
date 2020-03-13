using System;
using Velo.DependencyInjection;
using Velo.DependencyInjection.Dependencies;
using Velo.DependencyInjection.Factories;
using Velo.DependencyInjection.Resolvers;
using Velo.Utils;

namespace Velo.CQRS.Queries.Pipeline
{
    internal sealed class QueryPipelineFactory : IDependencyFactory
    {
        private readonly Type _contract;

        private readonly Type _behaviourType;
        private readonly Type _preProcessorType;
        private readonly Type _postProcessorType;

        private readonly Type _fullPipeline;
        private readonly Type _simplePipeline;
        private readonly Type _sequentialPipeline;

        public QueryPipelineFactory()
        {
            _contract = typeof(IQueryPipeline<,>);

            _behaviourType = typeof(IQueryBehaviour<,>);
            _preProcessorType = typeof(IQueryPreProcessor<,>);
            _postProcessorType = typeof(IQueryPostProcessor<,>);

            _fullPipeline = typeof(QueryFullPipeline<,>);
            _simplePipeline = typeof(QuerySimplePipeline<,>);
            _sequentialPipeline = typeof(QuerySequentialPipeline<,>);
        }

        public bool Applicable(Type contract)
        {
            return ReflectionUtils.IsGenericTypeImplementation(contract, _contract);
        }

        public IDependency BuildDependency(Type contract, IDependencyEngine engine)
        {
            var genericArgs = contract.GenericTypeArguments;

            Type pipelineType;
            if (ExistsBehaviour(genericArgs, engine)) pipelineType = _fullPipeline;
            else if (ExistsPreOrPostProcessor(genericArgs, engine)) pipelineType = _sequentialPipeline;
            else pipelineType = _simplePipeline;

            var implementation = pipelineType.MakeGenericType(genericArgs);
            var lifetime = engine.DefineLifetime(implementation);
            var resolver = DependencyResolver.Build(lifetime, implementation, engine);

            return Dependency.Build(lifetime, new[] {contract}, resolver);
        }

        private bool ExistsBehaviour(Type[] contractGenericArgs, IDependencyEngine engine)
        {
            var behaviourType = _behaviourType.MakeGenericType(contractGenericArgs);
            return engine.Contains(behaviourType);
        }

        private bool ExistsPreOrPostProcessor(Type[] contractGenericArgs, IDependencyEngine engine)
        {
            var preProcessorType = _preProcessorType.MakeGenericType(contractGenericArgs);
            if (engine.Contains(preProcessorType)) return true;

            var postProcessorType = _postProcessorType.MakeGenericType(contractGenericArgs);
            return engine.Contains(postProcessorType);
        }
    }
}