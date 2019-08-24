using System.Collections.Generic;
using Velo.Dependencies;
using Velo.Mapping;
using Velo.Serialization;
using Velo.TestsModels;
using Velo.TestsModels.Services;
using Xunit;

namespace Velo
{
    public class DependencyTests
    {
        [Fact]
        public void Factory_Activator()
        {
            var container = new DependencyBuilder()
                .AddSingleton<JConverter>()
                .AddFactory<ISession, Session>()
                .BuildContainer();

            var first = container.Resolve<ISession>();
            var second = container.Resolve<ISession>();

            Assert.NotSame(first, second);
        }

        [Fact]
        public void Factory_Array()
        {
            var container = new DependencyBuilder()
                .AddFactory<ISession, Session>()
                .AddSingleton<JConverter>()
                .AddSingleton<IConfiguration, Configuration>()
                .Configure(dataRepository => dataRepository
                    .Contracts<IRepository, IDataRepository>()
                    .Implementation<DataRepository>()
                    .Singleton())
                .Configure(userRepository => userRepository
                    .Contracts<IRepository, IUserRepository>()
                    .Implementation<UserRepository>()
                    .Singleton())
                .BuildContainer();

            var first = container.Resolve<IRepository[]>();
            var second = container.Resolve<IRepository[]>();

            Assert.NotSame(first, second);

            for (var i = 0; i < first.Length; i++)
            {
                var firstRepository = first[i];
                var secondRepository = second[i];

                Assert.Same(firstRepository, secondRepository);
                Assert.Same(firstRepository.Configuration, secondRepository.Configuration);
                Assert.Same(firstRepository.Session, secondRepository.Session);
            }
        }

        [Fact]
        public void Factory_Builder()
        {
            var container = new DependencyBuilder()
                .AddSingleton<JConverter>()
                .AddFactory<ISession>(ctx => new Session(ctx.Resolve<JConverter>()))
                .BuildContainer();

            var first = container.Resolve<ISession>();
            var second = container.Resolve<ISession>();

            Assert.NotSame(first, second);
        }

        [Fact]
        public void Factory_Generic()
        {
            var container = new DependencyBuilder()
                .AddGenericFactory(typeof(List<>))
                .BuildContainer();

            var first = container.Resolve<List<int>>();
            var second = container.Resolve<List<int>>();

            Assert.NotSame(first, second);
        }

        [Fact]
        public void Resolve()
        {
            var container = new DependencyBuilder()
                .AddSingleton<JConverter>()
                .AddSingleton<ILogger, Logger>()
                .AddSingleton<IMapper<Boo>, CompiledMapper<Boo>>()
                .AddSingleton<IMapper<Foo>, CompiledMapper<Foo>>()
                .AddSingleton<IConfiguration>(provider => new Configuration())
                .AddFactory<ISession, Session>()
                .AddSingleton<IDataService, DataService>()
                .AddSingleton<IDataRepository, DataRepository>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IUserRepository, UserRepository>()
                .AddSingleton<SomethingController>()
                .BuildContainer();

            var controller = container.Resolve<SomethingController>();
            Assert.NotNull(controller);
        }

        [Fact]
        public void Scan()
        {
            var container = new DependencyBuilder()
                .AddSingleton<IConfiguration, Configuration>()
                .AddFactory<ISession, Session>()
                .AddSingleton<JConverter>()
                .Scan(scanner => scanner
                    .Assembly(typeof(IRepository).Assembly)
                    .RegisterAsSingleton<IRepository>())
                .BuildContainer();

            var repositories = container.Resolve<IRepository[]>();
            Assert.Equal(2, repositories.Length);
            Assert.Contains(repositories, r => r.GetType() == typeof(DataRepository));
            Assert.Contains(repositories, r => r.GetType() == typeof(UserRepository));
        }

        [Fact]
        public void Singleton_Activator()
        {
            var container = new DependencyBuilder()
                .AddSingleton<JConverter>()
                .AddSingleton<ISession, Session>()
                .BuildContainer();

            var first = container.Resolve<ISession>();
            var second = container.Resolve<ISession>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Singleton_Builder()
        {
            var container = new DependencyBuilder()
                .AddSingleton<JConverter>()
                .AddSingleton<ISession>(ctx => new Session(ctx.Resolve<JConverter>()))
                .BuildContainer();

            var first = container.Resolve<ISession>();
            var second = container.Resolve<ISession>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Singleton_Generic()
        {
            var container = new DependencyBuilder()
                .AddGenericSingleton(typeof(CompiledMapper<>))
                .BuildContainer();

            var first = container.Resolve<CompiledMapper<Boo>>();
            var second = container.Resolve<CompiledMapper<Boo>>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Singleton_Instance()
        {
            var container = new DependencyBuilder()
                .AddSingleton(new JConverter())
                .BuildContainer();

            var first = container.Resolve<JConverter>();
            var second = container.Resolve<JConverter>();

            Assert.Same(first, second);
        }
    }
}