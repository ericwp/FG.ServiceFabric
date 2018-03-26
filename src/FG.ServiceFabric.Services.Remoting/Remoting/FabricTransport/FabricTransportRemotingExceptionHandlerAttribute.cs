﻿using System;

namespace FG.ServiceFabric.Services.Remoting.FabricTransport
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    internal sealed class FabricTransportRemotingExceptionHandlerAttribute : Attribute
    {
        public FabricTransportRemotingExceptionHandlerAttribute(Type exceptionHandlerType)
        {
            ExceptionHandlerType = exceptionHandlerType;
        }

        public Type ExceptionHandlerType { get; }
    }
}