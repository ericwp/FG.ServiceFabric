/*******************************************************************************************
*  This class is autogenerated from the class Services\Runtime\StateSession\CosmosDb\Diagnostics\FGServiceFabricPersistenceEventSource.eventsource.json
*  Do not directly update this class as changes will be lost on rebuild.
*******************************************************************************************/
using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace FG.ServiceFabric.Services.Runtime.StateSession.CosmosDb.Diagnostics
{
	[EventSource(Name = "FG-ServiceFabric")]
	internal sealed partial class FGServiceFabricPersistenceEventSource : EventSource
	{
		public static readonly FGServiceFabricPersistenceEventSource Current = new FGServiceFabricPersistenceEventSource();

		static FGServiceFabricPersistenceEventSource()
		{
			// A workaround for the problem where ETW activities do not 
			// get tracked until Tasks infrastructure is initialized.
			// This problem will be fixed in .NET Framework 4.6.2.
			Task.Run(() => { });
		}

		// Instance constructor is private to enforce singleton semantics
		private FGServiceFabricPersistenceEventSource() : base() { }

		#region Keywords
		// Event keywords can be used to categorize events. 
		// Each keyword is a bit flag. A single event can be 
		// associated with multiple keywords (via EventAttribute.Keywords property).
		// Keywords must be defined as a public class named 'Keywords' 
		// inside EventSource that uses them.
		public static class Keywords
		{
			public const EventKeywords DocumentDbStateSessionManager = (EventKeywords)0x1L;
			public const EventKeywords DocumentDbStateSession = (EventKeywords)0x2L;
			public const EventKeywords Error = (EventKeywords)0x4L;

		}
		#endregion Keywords

		#region Tasks

		public static class Tasks
		{

		}
		#endregion Tasks

		#region Events



		#endregion Events
	}


	internal static class FGServiceFabricPersistenceEventSourceHelpers
	{

            public static string AsJson(this object that)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(that);
            }


	}

}