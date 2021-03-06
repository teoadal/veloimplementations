using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Moq;
using Velo.ECS.Actors.Context;
using Velo.ECS.Actors.Groups;
using Velo.ECS.Components;
using Velo.TestsModels.ECS;
using Xunit;

namespace Velo.Tests.ECS.Actors
{
    public class ActorGroupShould : ECSTestClass
    {
        private readonly TestActor _actor;
        private readonly Mock<IActorContext> _actorContext;
        private readonly IActorGroup<TestActor> _actorGroup;

        public ActorGroupShould()
        {
            _actor = new TestActor(1);
            _actorContext = new Mock<IActorContext>();
            _actorGroup = new ActorGroup<TestActor>(_actorContext.Object);
            
            InjectComponentsArray(Array.Empty<IComponent>());
        }

        [Fact]
        public void AddActor()
        {
            RaiseActorAdded(_actorContext, _actor);

            _actorGroup.Should().Contain(_actor);
        }

        [Fact]
        public void Contains()
        {
            RaiseActorAdded(_actorContext, _actor);

            _actorGroup.Contains(_actor.Id).Should().BeTrue();
        }

        [Theory]
        [AutoData]
        public void Enumerable(int length)
        {
            for (var i = 0; i < length; i++)
            {
                RaiseActorAdded(_actorContext, new TestActor(i));
            }

            var exists = new HashSet<int>();

            foreach (var actor in _actorGroup)
            {
                exists.Add(actor.Id).Should().BeTrue();
            }

            exists.Count.Should().Be(length);
        }

        [Fact]
        public void EnumerableWhere()
        {
            foreach (var actor in Fixture.CreateMany<TestActor>())
            {
                RaiseActorAdded(_actorContext, actor);
                
                _actorGroup
                    .Where((a, id) => a.Id == id, actor.Id)
                    .Should().ContainSingle(a => a.Id == actor.Id);    
            }
        }

        [Theory]
        [AutoData]
        public void HasLength(int length)
        {
            for (var i = 0; i < length; i++)
            {
                RaiseActorAdded(_actorContext, _actor);
            }

            _actorGroup.Length.Should().Be(length);
        }

        [Fact]
        public void NotAddActor()
        {
            var actorId = _actor.Id + 1;

            RaiseActorAdded(_actorContext, BuildActor(actorId));

            _actorGroup.Contains(actorId).Should().BeFalse();
        }

        [Fact]
        public void RaiseAdded()
        {
            using var actorGroup = _actorGroup.Monitor();

            RaiseActorAdded(_actorContext, _actor);

            actorGroup.Should().Raise(nameof(IActorGroup<TestActor>.Added));
        }

        [Fact]
        public void RaiseRemoved()
        {
            using var actorGroup = _actorGroup.Monitor();

            RaiseActorAdded(_actorContext, _actor);
            RaiseActorRemoved(_actorContext, _actor);

            actorGroup.Should().Raise(nameof(IActorGroup<TestActor>.Removed));
        }

        [Fact]
        public void RemoveActor()
        {
            RaiseActorAdded(_actorContext, _actor);
            RaiseActorRemoved(_actorContext, _actor);

            _actorGroup.Should().NotContain(_actor);
        }

        [Fact]
        public void TryGetTrue()
        {
            RaiseActorAdded(_actorContext, _actor);

            _actorGroup.TryGet(_actor.Id, out var exists).Should().BeTrue();
            exists.Should().Be(_actor);
        }

        [Fact]
        public void TryGetFalse()
        {
            _actorGroup.TryGet(-_actor.Id, out _).Should().BeFalse();
        }
    }
}