﻿using FG.ServiceFabric.Testing.Mocks;
using FG.ServiceFabric.Tests.EventStoredActor;

namespace FG.ServiceFabric.Tests
{
    public class ForTestEventStoredActor
    {
        public static void Setup(MockFabricRuntime mockFabricRuntime)
        {
            mockFabricRuntime.SetupActor(
                (service, id) =>
                    new EventStoredActor.EventStoredActor(
                        actorService: service,
                        actorId: id),
                (context, actorTypeInformation, stateProvider, stateManagerFactory) =>
                    new EventStoredActorService(
                        context: context,
                        actorTypeInfo: actorTypeInformation,
                        stateProvider: stateProvider,
                        stateManagerFactory: stateManagerFactory),
                serviceDefinition: MockServiceDefinition.CreateUniformInt64Partitions(1));
        }
    }
}