using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Velo.CQRS;
using Velo.TestsModels.Boos;
using Velo.TestsModels.Emitting.Plus;

namespace Velo.Benchmark.CQRS
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    [MarkdownExporterAttribute.GitHub]
    [MeanColumn, MemoryDiagnoser]
    public class MediatorNotificationBenchmark
    {
        [Params(1, 1000)] 
        public int Count;

        [Params(1, 5)] 
        public int ProcessorsCount;
        
        private IMediator _mediator;
        private Notification[] _mediatorNotifications;

        private IEmitter _emitter;
        private PlusNotification[] _emitterNotifications;

        [GlobalSetup]
        public void Init()
        {
            _emitterNotifications = new PlusNotification[Count];
            _mediatorNotifications = new Notification[Count];

            for (var i = 0; i < Count; i++)
            {
                _emitterNotifications[i] = new PlusNotification();
                _mediatorNotifications[i] = new Notification();
            }

            var repository = new BooRepository(null);

            _mediator = MediatorBuilder.BuildMediatR(repository, services => services
                .AddSingleton<INotificationHandler<Notification>, NotificationHandler>(), ProcessorsCount);
            
            _emitter = MediatorBuilder.BuildEmitter(repository, services => services
                .AddNotificationProcessor<PlusNotificationProcessor>(), ProcessorsCount);
        }

        [Benchmark(Baseline = true)]
        public async Task<long> MediatR()
        {
            var stub = 0L;
            foreach (var notification in _mediatorNotifications)
            {
                await _mediator.Publish(notification);
                stub += notification.Counter;
            }
        
            return stub;
        }

        [Benchmark]
        public async Task<long> Emitter()
        {
            var stub = 0L;
            foreach (var notification in _emitterNotifications)
            {
                await _emitter.Publish(notification);
                stub += notification.Counter;
            }

            return stub;
        }
    }
}