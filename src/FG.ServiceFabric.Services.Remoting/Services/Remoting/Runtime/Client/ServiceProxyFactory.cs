﻿namespace FG.ServiceFabric.Services.Remoting.Runtime.Client
{
    using System;

    using FG.ServiceFabric.Diagnostics;

    using Microsoft.ServiceFabric.Services.Client;
    using Microsoft.ServiceFabric.Services.Communication.Client;
    using Microsoft.ServiceFabric.Services.Remoting;
    using Microsoft.ServiceFabric.Services.Remoting.Client;

    public class ServiceProxyFactory : ServiceProxyFactoryBase, IServiceProxyFactory
    {
        public ServiceProxyFactory(IServiceClientLogger logger)
            : base(logger)
        {
        }

        public TServiceInterface CreateServiceProxy<TServiceInterface>(
            Uri serviceUri,
            ServicePartitionKey partitionKey = null,
            TargetReplicaSelector targetReplicaSelector = TargetReplicaSelector.Default,
            string listenerName = null)
            where TServiceInterface : IService
        {
            this.GetOrDiscoverServiceMethodDispatcher(typeof(TServiceInterface));
            var proxy = this.GetInnerServiceProxyFactory(typeof(TServiceInterface))
                .CreateServiceProxy<TServiceInterface>(serviceUri, partitionKey, targetReplicaSelector, listenerName);
            UpdateRequestContext(serviceUri);
            return proxy;
        }
    }
}