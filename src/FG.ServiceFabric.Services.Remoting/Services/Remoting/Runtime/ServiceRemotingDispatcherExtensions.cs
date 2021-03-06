﻿using System;
using System.Diagnostics;
using System.Reflection;

namespace FG.ServiceFabric.Services.Remoting.Runtime
{
    public static class ServiceRemotingDispatcherExtensions
    {
        public static string GetMethodDispatcherMapName(this Microsoft.ServiceFabric.Services.Remoting.Runtime.ServiceRemotingDispatcher that, int interfaceId, int methodId)
        {
            try
            {
                var methodDispatcherMapFieldInfo = typeof(Microsoft.ServiceFabric.Services.Remoting.Runtime.ServiceRemotingDispatcher).GetField("methodDispatcherMap",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
                var methodDispatcherMap = methodDispatcherMapFieldInfo?.GetValue(that);
                var methodDispatcher = methodDispatcherMap?.GetType()
                    .InvokeMember("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, methodDispatcherMap,
                        new object[] {interfaceId});
                var getMethodNameMethodInfo =
                    methodDispatcher?.GetType().GetInterface("Microsoft.ServiceFabric.Services.Remoting.IMethodDispatcher").GetMethod("GetMethodName");
                //var getMethodNameMethodInfo = methodDispatcher?.GetType()
                //    .GetMethod("Microsoft.ServiceFabric.Services.Remoting.IMethodDispatcher.GetMethodName", BindingFlags.NonPublic | BindingFlags.Instance);
                var methodName = getMethodNameMethodInfo?.Invoke(methodDispatcher, new object[] {methodId}) as string;
                return methodName;
            }
            catch (Exception)
            {
                // Ignore
                return null;
            }
        }
        
        public static string GetMethodDispatcherMapName(this Microsoft.ServiceFabric.Services.Remoting.Builder.MethodDispatcherBase that, int interfaceId, int methodId)
        {
            Debug.Assert(that.InterfaceId != interfaceId);
            return that.GetMethodName(methodId);
        }
    }
}