﻿using System;
using System.Threading.Tasks;
using FG.CQRS;
using FG.ServiceFabric.Actors;
using FG.ServiceFabric.Actors.Runtime;
using FG.ServiceFabric.Tests.EventStoredActor.Interfaces;
using FluentAssertions;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace FG.ServiceFabric.Tests.CQRS.MessageChannelTests
{
    public class TestMessageEndpoint : IReliableMessageEndpoint<ICommand>
    {
        public Task HandleMessageAsync<TMessage>(TMessage message) where TMessage : ICommand
        {
            throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class When_receiving_a_reliable_message : IReliableMessageEndpoint<ICommand>
    {
        [SetUp]
        public async Task ReceiveMessage()
        {
            _inboundChannel = new InboundReliableMessageChannel<ICommand>(this);
            _command = new IndexCommand {PersonId = Guid.NewGuid()};
            await _inboundChannel.ReceiveMessageAsync(ReliableMessage.Create(_command));
        }

        private InboundReliableMessageChannel<ICommand> _inboundChannel;
        private ICommand _handledMessage;
        private IndexCommand _command;

        public Task HandleMessageAsync<TMessage>(TMessage message) where TMessage : ICommand
        {
            _handledMessage = message;
            return Task.FromResult(true);
        }

        [Test]
        public void Then_message_is_dispatched_to_handler()
        {
            _handledMessage.ShouldBeEquivalentTo(_command);
        }
    }
}