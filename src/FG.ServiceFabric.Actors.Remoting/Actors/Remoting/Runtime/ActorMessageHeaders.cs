﻿using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting.V1;

namespace FG.ServiceFabric.Actors.Remoting.Runtime
{
    /// <summary>
    ///     Header for the actor messages.
    ///     Copied from internal implementation in Microsoft.ServiceFabric.Actors, Version=5.0.0.0,
    /// </summary>
    [DataContract(Name = "addr", Namespace = "urn:actors")]
    public class ActorMessageHeaders
    {
        public ServiceRemotingMessageHeaders ToServiceMessageHeaders()
        {
            var remotingMessageHeaders = new ServiceRemotingMessageHeaders();
            remotingMessageHeaders.AddHeader(ActorMessageHeaderName, Serialize());
            return remotingMessageHeaders;
        }

        public static bool TryFromServiceMessageHeaders(ServiceRemotingMessageHeaders headers,
            out ActorMessageHeaders actorHeaders)
        {
            actorHeaders = null;
            byte[] headerValue;
            if (!headers.TryGetHeaderValue(ActorMessageHeaderName, out headerValue))
                return false;
            actorHeaders = Deserialize(headerValue);
            return true;
        }

        private byte[] Serialize()
        {
            using (var memoryStream = new MemoryStream())
            {
                var binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(memoryStream);
                Serializer.WriteObject(binaryWriter, this);
                binaryWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        private static ActorMessageHeaders Deserialize(byte[] headerBytes)
        {
            using (var memoryStream = new MemoryStream(headerBytes))
            {
                var binaryReader =
                    XmlDictionaryReader.CreateBinaryReader(memoryStream, XmlDictionaryReaderQuotas.Max);
                return (ActorMessageHeaders) Serializer.ReadObject(binaryReader);
            }
        }
#pragma warning disable 0649
        private static readonly DataContractSerializer Serializer =
            new DataContractSerializer(typeof(ActorMessageHeaders));
        private const string ActorMessageHeaderName = "ActorMessageHeader";

        [DataMember(IsRequired = true, Order = 0)] public int InterfaceId;

        [DataMember(IsRequired = true, Order = 1)] public int MethodId;

        [DataMember(IsRequired = false, Order = 2)] public ActorId ActorId;

        [DataMember(IsRequired = false, Order = 3)] public string CallContext;
#pragma warning restore 0649
    }
}