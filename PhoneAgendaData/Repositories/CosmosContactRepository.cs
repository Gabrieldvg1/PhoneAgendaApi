using Microsoft.Azure.Cosmos;
using PhoneAgendaData.Models;

namespace PhoneAgendaData.Repositories;
public class CosmosContactRepository : IContactRepository
{
    private readonly CosmosClient _client;
    private readonly Container _container;

    public CosmosContactRepository(string connectionString, string databaseName, string containerName)
    {
        _client = new CosmosClient(connectionString);
        var database = _client.GetDatabase(databaseName);
        _container = database.GetContainer(containerName);
    }

    public async Task<Contact> CreateAsync(Contact contact)
    {
        var response = await _container.CreateItemAsync(contact, new PartitionKey(contact.Id));
        return response.Resource;
    }

    public async Task<IEnumerable<Contact>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Contact>("SELECT * FROM c");
        var results = new List<Contact>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Contact?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Contact>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Contact> UpdateAsync(Contact contact)
    {
        var response = await _container.ReplaceItemAsync(contact, contact.Id, new PartitionKey(contact.Id));
        return response.Resource;
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<Contact>(id, new PartitionKey(id));
    }
}

