using System;
using System.Threading.Tasks;
using FG.ServiceFabric.Utils;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace FG.ServiceFabric.DocumentDb.CosmosDb
{
    public class CosmosDbStateSession : IDocumentDbStateWriter, IDocumentDbStateReader
    {
        private readonly string _collection;
        private readonly string _databaseName;
        private readonly DocumentClient _documentClient;
        
        public CosmosDbStateSession(
			ISettingsProvider settingsProvider, 
			ICosmosDbClientFactory factory = null, 
			ConnectionPolicySetting connectionPolicySetting = ConnectionPolicySetting.GatewayHttps)
        {
            factory = factory ?? new CosmosDbClientFactory();

            _collection = settingsProvider["Collection"];
            _databaseName = settingsProvider["DatabaseName"];
            var endpointiUri = settingsProvider["EndpointUri"];
            var primaryKey = settingsProvider["PrimaryKey"];
            
            _documentClient = factory.OpenAsync( // TODO: Do proper init.
                databaseName: _databaseName,
                collection: new CosmosDbCollectionDefinition(_collection, $"/partitionKey"),
                endpointUri: new Uri(endpointiUri),
                primaryKey: primaryKey,
                connectionPolicySetting: connectionPolicySetting).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _documentClient?.Dispose();
            }
        }
        
        public async Task UpsertAsync<T>(T state, IStateMetadata metadata) where T : IPersistedIdentity
        {
            var document = new StateWrapper<T>(state.Id, state, metadata);
            await _documentClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(_databaseName, _collection), document);
        }
        
        public async Task DelecteAsync(string id, Guid partitionKey)
        {
            await _documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(_databaseName, _collection, id), new RequestOptions { PartitionKey = new PartitionKey(partitionKey)});
        }
        
        public async Task<T> ReadAsync<T>(string id, Guid partitionKey) where T : IPersistedIdentity
        {
            var response = await _documentClient.ReadDocumentAsync<StateWrapper<T>>(UriFactory.CreateDocumentUri(_databaseName, _collection, id), new RequestOptions { PartitionKey = new PartitionKey(partitionKey) });
            return response.Document.State;
        }
    }
}