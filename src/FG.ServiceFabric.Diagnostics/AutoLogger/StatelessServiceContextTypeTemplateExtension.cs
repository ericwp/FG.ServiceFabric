﻿using System.Fabric;
using FG.Diagnostics.AutoLogger.Model;

namespace FG.ServiceFabric.Diagnostics.AutoLogger
{
    public class StatelessServiceContextTypeTemplateExtension : BaseTemplateExtension<System.Fabric.StatelessServiceContext>
    {       
        protected override void BuildArguments(TypeTemplate<StatelessServiceContext> config)
        {
            config
                .AddArgument(x => x.ServiceName.ToString())
                .AddArgument(x => x.ServiceTypeName)
                .AddArgument("replicaOrInstanceId", x => x.InstanceId)
                .AddArgument(x => x.PartitionId)
                .AddArgument(x => x.CodePackageActivationContext.ApplicationName)
                .AddArgument(x => x.CodePackageActivationContext.ApplicationTypeName)
                .AddArgument(x => x.NodeContext.NodeName)
                ;
        }

        public override string Module => @"ServiceFabric";        
    }
}