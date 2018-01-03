/*******************************************************************************************
*  This class is autogenerated from the class ActorLogger
*  Do not directly update this class as changes will be lost on rebuild.
*******************************************************************************************/

using System;
using System.Diagnostics.Tracing;
using Microsoft.ServiceFabric.Actors;

namespace FG.ServiceFabric.Tests.EventStoredActor
{
    internal sealed partial class FGServiceFabricTestsEventStoredActorEventSource
    {
        private const int FailedToSendMessageEventId = 1001;


        private const int MessageSentEventId = 2002;


        private const int MessageMovedToDeadLetterQueueEventId = 3003;

        [Event(FailedToSendMessageEventId, Level = EventLevel.LogAlways, Message = "{4}", Keywords = Keywords.Actor)]
        private void FailedToSendMessage(
            bool autogenerated,
            string machineName,
            string actorId,
            string serviceUri,
            string message,
            string source,
            string exceptionTypeName,
            string exception)
        {
            WriteEvent(
                FailedToSendMessageEventId,
                autogenerated,
                machineName,
                actorId,
                serviceUri,
                message,
                source,
                exceptionTypeName,
                exception);
        }

        [NonEvent]
        public void FailedToSendMessage(
            bool autogenerated,
            string machineName,
            ActorId actorId,
            Uri serviceUri,
            Exception ex)
        {
            if (IsEnabled())
                FailedToSendMessage(
                    autogenerated,
                    Environment.MachineName,
                    actorId.ToString(),
                    serviceUri.ToString(),
                    ex.Message,
                    ex.Source,
                    ex.GetType().FullName,
                    ex.AsJson());
        }

        [Event(MessageSentEventId, Level = EventLevel.LogAlways, Message = "Message Sent {2} {3} {4} {5}",
            Keywords = Keywords.Actor)]
        private void MessageSent(
            bool autogenerated,
            string machineName,
            string actorId,
            string serviceUri,
            string messageType,
            string messagePayload)
        {
            WriteEvent(
                MessageSentEventId,
                autogenerated,
                machineName,
                actorId,
                serviceUri,
                messageType,
                messagePayload);
        }

        [NonEvent]
        public void MessageSent(
            bool autogenerated,
            string machineName,
            ActorId actorId,
            Uri serviceUri,
            string messageType,
            string messagePayload)
        {
            if (IsEnabled())
                MessageSent(
                    autogenerated,
                    Environment.MachineName,
                    actorId.ToString(),
                    serviceUri.ToString(),
                    messageType,
                    messagePayload);
        }

        [Event(MessageMovedToDeadLetterQueueEventId, Level = EventLevel.LogAlways,
            Message = "Message Moved To Dead Letter Queue {2} {3} {4}", Keywords = Keywords.Actor)]
        public void MessageMovedToDeadLetterQueue(
            bool autogenerated,
            string machineName,
            string messageType,
            string messagePayload,
            int deadLetterQueueDepth)
        {
            WriteEvent(
                MessageMovedToDeadLetterQueueEventId,
                autogenerated,
                machineName,
                messageType,
                messagePayload,
                deadLetterQueueDepth);
        }
    }
}