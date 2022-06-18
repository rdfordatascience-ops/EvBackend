using Microsoft.Azure.Cosmos;

public class CosmosDbService : ICosmosDbService
    {
        private Container _container;
        private string _dataBaseName;
        private CosmosClient _cosmosDbClient;
        public CosmosDbService(string databaseName, CosmosClient cosmosDbClient)
        {
            _dataBaseName = databaseName;
            _cosmosDbClient = cosmosDbClient;
        }

        public async Task AddAsync(string id,dynamic item, string containerName)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            await _container.CreateItemAsync(item);
        }

        public async Task DeleteAsync(string id,string containerName)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            await _container.DeleteItemAsync<dynamic>(id, new PartitionKey(id));
        }

        public async Task<dynamic> GetAsync(string id,string containerName)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            var response = await _container.ReadItemAsync<dynamic>(id, new PartitionKey(id));
            return response.Resource;
        }

        public async Task<List<dynamic>> GetMultipleAsync(string queryString,string containerName)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            var query = _container.GetItemQueryIterator<dynamic>(new QueryDefinition(queryString));
            var results = new List<dynamic>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results.ToList();
        }

        public async Task UpdateAsync(string id, dynamic item,string containerName)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            await _container.UpsertItemAsync(item, new PartitionKey(id));
        }
        public async Task UpdateAsyncv2(string id, dynamic item, string containerName,string queryString)
        {
            _container = _cosmosDbClient.GetContainer(_dataBaseName, containerName);
            var query = _container.GetItemQueryIterator<dynamic>(new QueryDefinition(queryString));
            var results = new List<dynamic>();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            if (results.Any())
            {
                var idfordel = results.FirstOrDefault().id.ToString();
                await _container.DeleteItemAsync<dynamic>(idfordel, new PartitionKey(id));
                await _container.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(id));
            }
            else
            {
                await _container.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(id));
            }
        }
    }