/*******************************************************************************************
*  This class is autogenerated from the class DocumentDbStateSessionManagerLogger
*  Do not directly update this class as changes will be lost on rebuild.
*******************************************************************************************/

using System;
using System.Collections.Generic;
using FG.ServiceFabric.Services.Runtime.StateSession.CosmosDb;

namespace FG.ServiceFabric
{
    internal sealed class DocumentDbStateSessionManagerLogger : IDocumentDbStateSessionManagerLogger
    {
        private readonly string _managerInstance;

        public DocumentDbStateSessionManagerLogger(
            string managerInstance)
        {
            _managerInstance = managerInstance;
        }

        public void StartingManager(
            string serviceName,
            Guid partitionId,
            string partitionKey,
            string endpointUri,
            string databaseName,
            string collection)
        {
            FGServiceFabricPersistenceEventSource.Current.StartingManager(
                _managerInstance,
                serviceName,
                partitionId,
                partitionKey,
                endpointUri,
                databaseName,
                collection
            );
        }


        public void CreatingCollection(
            string collectionName)
        {
            FGServiceFabricPersistenceEventSource.Current.CreatingCollection(
                _managerInstance,
                collectionName
            );
        }


        public void CreatingClient(
        )
        {
            FGServiceFabricPersistenceEventSource.Current.CreatingClient(
                _managerInstance
            );
        }


        public void CreatingSession(
        )
        {
            FGServiceFabricPersistenceEventSource.Current.CreatingSession(
                _managerInstance
            );
        }

        private sealed class ScopeWrapper : IDisposable
        {
            private readonly IEnumerable<IDisposable> _disposables;

            public ScopeWrapper(IEnumerable<IDisposable> disposables)
            {
                _disposables = disposables;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                    foreach (var disposable in _disposables)
                        disposable.Dispose();
            }
        }

        private sealed class ScopeWrapperWithAction : IDisposable
        {
            private readonly Action _onStop;

            public ScopeWrapperWithAction(Action onStop)
            {
                _onStop = onStop;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            internal static IDisposable Wrap(Func<IDisposable> wrap)
            {
                return wrap();
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                    _onStop?.Invoke();
            }
        }
    }
}