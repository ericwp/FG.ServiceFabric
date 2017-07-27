﻿using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using FG.CQRS;
using FG.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors;

namespace FG.ServiceFabric.Tests.EventStoredActor.Interfaces
{
    #region Contracts

    public interface IEventStoredActor : IActor
    {
        Task CreateAsync(CreateCommand command);
    }

    public interface IEventStoredActorService : FG.ServiceFabric.Actors.Runtime.IEventStoredActorService
    {
        Task<ReadModel> GetAsync(Guid aggregateRootId);
    }

    #endregion

    #region Commands
    [DataContract]
    public class CreateCommand : DomainCommandBase
    {
        [DataMember]
        public string SomeProperty { get; set; }
    }

    public class AddChildCommand : DomainCommandBase
    {
        [DataMember]
        public Guid AggretateRootId { get; set; }
        [DataMember]
        public string ChildProperty { get; set; }
    }
    #endregion

    #region Models
   
    [DataContract]
    public class ReadModel : IAggregateReadModel
    {
        public Guid Id { get; set; }
        [DataMember]
        public string SomeProperty { get; set; }
    }
    #endregion
}