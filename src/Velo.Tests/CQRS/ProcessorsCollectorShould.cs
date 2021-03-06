using FluentAssertions;
using Velo.DependencyInjection;
using Xunit;

namespace Velo.Tests.CQRS
{
    public class ProcessorsCollectorShould : CQRSTestClass
    {
        private readonly DependencyCollection _dependencies;

        public ProcessorsCollectorShould()
        {
            _dependencies = new DependencyCollection()
                .Scan(scanner => scanner
                    .AssemblyOf<TestsModels.Emitting.Boos.Create.Processor>()
                    .RegisterEmitterProcessors());
        }

        [Fact]
        public void FindBehaviours()
        {
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Get.Behaviour)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.MeasureBehaviour)).Should().BeTrue();
        }

        [Fact]
        public void FindPreProcessors()
        {
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Create.PreProcessor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Get.PreProcessor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Create.NotificationProcessor)).Should().BeTrue();
        }

        [Fact]
        public void FindProcessors()
        {
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Create.Processor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Get.Processor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Update.Processor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Foos.Create.Processor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Foos.Create.OnBooCreated)).Should().BeTrue();
        }

        [Fact]
        public void FindPostProcessors()
        {
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Create.PostProcessor)).Should().BeTrue();
            _dependencies.Contains(typeof(TestsModels.Emitting.Boos.Get.PostProcessor)).Should().BeTrue();
        }
    }
}