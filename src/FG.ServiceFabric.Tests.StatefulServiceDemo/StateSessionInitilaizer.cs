﻿using System;
using System.Fabric;
using FG.ServiceFabric.Fabric;
using FG.ServiceFabric.Services.Runtime.StateSession;
using FG.ServiceFabric.Services.Runtime.StateSession.InMemory;
using FG.ServiceFabric.Testing.Mocks;

namespace FG.ServiceFabric.Tests.StatefulServiceDemo
{
    public class StateSessionInitilaizer
    {
        public static IStateSessionManager CreateStateManager(ServiceContext context)
        {
            var partitionEnumerationManager = MockFabricRuntime.Current != null
                ? (() =>
                    MockFabricRuntime.Current.PartitionEnumerationManager)
                : (Func<IPartitionEnumerationManager>)(() =>
                    (IPartitionEnumerationManager)new FabricClientQueryManagerPartitionEnumerationManager(
                        new FabricClient()));
            return new InMemoryStateSessionManager(StateSessionHelper.GetServiceName(context.ServiceName),
                context.PartitionId,
                StateSessionHelper.GetPartitionInfo(context, partitionEnumerationManager).GetAwaiter().GetResult());
        }
    }
}