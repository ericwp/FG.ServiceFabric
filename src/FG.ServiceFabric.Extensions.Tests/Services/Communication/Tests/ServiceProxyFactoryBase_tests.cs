﻿using System.Fabric;
using System.Threading.Tasks;
using FG.Common.Utils;
using FG.ServiceFabric.Services.Remoting.Runtime.Client;
using FG.ServiceFabric.Tests.StatefulServiceDemo.With_multiple_states;
using FluentAssertions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Builder;
using Microsoft.ServiceFabric.Services.Runtime;
using NUnit.Framework;

namespace FG.ServiceFabric.Services.Communication.Tests
{
	public class ServiceProxyFactoryBase_tests
	{
		[Test]
		public void Should_be_able_to_get_the_Dispatcher()
		{
			var serviceType = typeof(IServiceForUnitTests);

			var dispatcher = typeof(ServiceProxyFactoryBase).CallPrivateStaticMethod<MethodDispatcherBase>("GetServiceMethodInformation", serviceType);

			dispatcher.Should().NotBeNull();
		}
	}

	public interface IServiceForUnitTests : IService
	{
		Task<string> GetValueAsync();
	}

	public class ServiceForUnitTests : StatefulService, IServiceForUnitTests
	{
		public ServiceForUnitTests(StatefulServiceContext serviceContext) : base(serviceContext)
		{
		}

		public ServiceForUnitTests(StatefulServiceContext serviceContext, IReliableStateManagerReplica2 reliableStateManagerReplica) : base(serviceContext, reliableStateManagerReplica)
		{
		}

		public Task<string> GetValueAsync()
		{
			return Task.FromResult("hello");
		}
	}
}