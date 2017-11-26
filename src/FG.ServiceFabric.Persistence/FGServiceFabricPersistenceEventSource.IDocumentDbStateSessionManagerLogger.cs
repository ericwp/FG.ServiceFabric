/*******************************************************************************************
*  This class is autogenerated from the class DocumentDbStateSessionManagerLogger
*  Do not directly update this class as changes will be lost on rebuild.
*******************************************************************************************/
using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace FG.ServiceFabric
{
	internal sealed partial class FGServiceFabricPersistenceEventSource
	{

		private const int StartingManagerEventId = 1001;

		[Event(StartingManagerEventId, Level = EventLevel.LogAlways, Message = "Starting Manager {1} {2} {3} {4} {5} {6}", Keywords = Keywords.DocumentDbStateSessionManager)]
		public void StartingManager(
			string managerInstance, 
			string serviceName, 
			Guid partitionId, 
			string partitionKey, 
			string endpointUri, 
			string databaseName, 
			string collection)
		{
			WriteEvent(
				StartingManagerEventId, 
				managerInstance, 
				serviceName, 
				partitionId, 
				partitionKey, 
				endpointUri, 
				databaseName, 
				collection);
		}


		private const int CreatingCollectionEventId = 1002;

		[Event(CreatingCollectionEventId, Level = EventLevel.LogAlways, Message = "Creating Collection {1}", Keywords = Keywords.DocumentDbStateSessionManager)]
		public void CreatingCollection(
			string managerInstance, 
			string collectionName)
		{
			WriteEvent(
				CreatingCollectionEventId, 
				managerInstance, 
				collectionName);
		}


		private const int CreatingClientEventId = 1003;

		[Event(CreatingClientEventId, Level = EventLevel.LogAlways, Message = "Creating Client", Keywords = Keywords.DocumentDbStateSessionManager)]
		public void CreatingClient(
			string managerInstance)
		{
			WriteEvent(
				CreatingClientEventId, 
				managerInstance);
		}


		private const int CreatingSessionEventId = 1004;

		[Event(CreatingSessionEventId, Level = EventLevel.LogAlways, Message = "Creating Session", Keywords = Keywords.DocumentDbStateSessionManager)]
		public void CreatingSession(
			string managerInstance)
		{
			WriteEvent(
				CreatingSessionEventId, 
				managerInstance);
		}


	}
}