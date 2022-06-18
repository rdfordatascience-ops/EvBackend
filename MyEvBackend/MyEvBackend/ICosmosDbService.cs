public interface ICosmosDbService
{
    Task<List<dynamic>> GetMultipleAsync(string query,string containerName);
    Task<dynamic> GetAsync(string id,string containerName);
    Task AddAsync(string id,dynamic item,string containerName);
    Task UpdateAsync(string id, dynamic item,string containerName);
    Task DeleteAsync(string id,string containerName);
    Task UpdateAsyncv2(string id, dynamic item, string containerName, string queryString);
}