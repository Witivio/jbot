using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Witivio.JBot.Core.Configuration;
using Witivio.JBot.Core.Infrastructure;

namespace Witivio.JBot.Core.Services
{
    public interface IPersistantDataStore : IDataStoreBase
    {

    }

    public interface IStatisticsDataStore : IDataStoreBase
    {

    }

    public interface IConversationDataStore : IDataStoreBase
    {

    }

    public interface IDataStore : IPersistantDataStore, IStatisticsDataStore, IConversationDataStore
    {

    }

    public interface IDataStoreBase
    {
        Task<bool> TryAddOrUpdateAsync<TValue>(string key, TValue value) where TValue : class;
        Task<TValue> GetValueAsync<TValue>(string key) where TValue : class;
        Task<IEnumerable<KeyValuePair<string, TValue>>> GetAllAsync<TValue>() where TValue : class;
        Task<bool> TryAddAsync<TValue>(TValue entity) where TValue : class;
        Task<bool> TryDeleteAsync(string key);
        Task TryDeleteBeforeDate(DateTimeOffset dateTimeOffset);

    }

    public class NullDataStore : IDataStore
    {
        public Task<IEnumerable<KeyValuePair<string, TValue>>> GetAllAsync<TValue>() where TValue : class
        {
            return Task.FromResult(Enumerable.Empty<KeyValuePair<string, TValue>>());
        }

        public Task<TValue> GetValueAsync<TValue>(string key) where TValue : class
        {
            return Task.FromResult(default(TValue));
        }

        public Task<bool> TryAddAsync<TValue>(TValue entity) where TValue : class
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryAddOrUpdateAsync<TValue>(string key, TValue value) where TValue : class
        {
            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteAsync(string key)
        {
            return Task.FromResult(true);
        }

        public Task TryDeleteBeforeDate(DateTimeOffset dateTimeOffset)
        {
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Provide state storage in Azure Table
    /// </summary>
    public class TableDataStore : IDataStore
    {
        public class Entity<TValue> : TableEntity
        {
            public Entity(string partitionKey, string key, TValue value)
            {
                this.PartitionKey = partitionKey;
                this.RowKey = key;
                this.StringValue = JsonConvert.SerializeObject(value);
            }

            public Entity() { }

            public string StringValue { get; set; }

            [IgnoreProperty]
            public TValue Value => JsonConvert.DeserializeObject<TValue>(StringValue, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            });
        }

        private readonly CloudTable _table;
        private bool _checkTable;
        private string _partitionKey;

        public TableDataStore(IConfiguration configuration, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(configuration.Get<string>(ConfigurationKeys.Storage.ConnectionString));
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference(tableName);

            _partitionKey = configuration.Get<string>(ConfigurationKeys.Credentials.BotId);

        }

        public async Task<bool> TryAddOrUpdateAsync<TValue>(string key, TValue value) where TValue : class
        {
            await EnsureTableExistAsync();
            var entity = new Entity<TValue>(_partitionKey, key, value);
            TableOperation insertOperation = TableOperation.InsertOrReplace(entity);
            var result = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteAsync(insertOperation));
            return result.HttpStatusCode == 204;
        }

        public async Task<bool> TryAddAsync<TValue>(TValue entity) where TValue : class
        {
            await EnsureTableExistAsync();
            TableOperation insertOperation = TableOperation.Insert(entity as TableEntity);
            var result = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteAsync(insertOperation));
            return result.HttpStatusCode == 204;
        }

        private async Task EnsureTableExistAsync()
        {
            if (!_checkTable)
            {
                await RetryPolicies.Retry.ExecuteAsync(_table.CreateIfNotExistsAsync);
                _checkTable = true;
            }
        }

        public async Task<TValue> GetValueAsync<TValue>(string key) where TValue : class
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<Entity<TValue>>(_partitionKey, key);
            TableResult retrievedResult = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteAsync(retrieveOperation));
            var result = retrievedResult.Result as Entity<TValue>;
            if (result != null)
                return result.Value;
            else
                return default(TValue);
        }


        public async Task<IEnumerable<KeyValuePair<string, TValue>>> GetAllAsync<TValue>() where TValue : class
        {
            await EnsureTableExistAsync();
            var items = new List<KeyValuePair<string, TValue>>();
            var query = new TableQuery<Entity<TValue>>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey));
            TableContinuationToken continuationToken = null;

            do
            {
                var token = continuationToken;
                var tableQueryResult = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteQuerySegmentedAsync(query, token));
                items.AddRange(tableQueryResult.Results.Where(s => s.Value != null).Select(s => new KeyValuePair<string, TValue>(s.RowKey, s.Value)));
                continuationToken = tableQueryResult.ContinuationToken;
            } while (continuationToken != null);

            return items;
        }

        public async Task<bool> TryDeleteAsync(string key)
        {
            TableOperation retrieveOperation = TableOperation.Retrieve<TableEntity>(_partitionKey, key);
            TableResult retrievedResult = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteAsync(retrieveOperation));
            var entity = retrievedResult.Result as TableEntity;

            TableOperation deleteOperation = TableOperation.Delete(entity);
            var result = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteAsync(deleteOperation));
            return result.HttpStatusCode == 204;
        }

        public async Task TryDeleteBeforeDate(DateTimeOffset dateTimeOffset)
        {
            var filter = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThanOrEqual, dateTimeOffset));

            var query = new TableQuery<TableEntity>().Where(filter);

            TableContinuationToken continuationToken = null;
            var items = new List<TableEntity>();
            do
            {
                var token = continuationToken;
                var tableQueryResult = await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteQuerySegmentedAsync(query, token));
                items.AddRange(tableQueryResult.Results);
                continuationToken = tableQueryResult.ContinuationToken;
            } while (continuationToken != null);

            TableBatchOperation batch = new TableBatchOperation();
            items.ForEach(s =>
            {
                s.ETag = "*";
                batch.Delete(s);
            });

            if (batch.Count > 0)
                await RetryPolicies.Retry.ExecuteAsync(() => _table.ExecuteBatchAsync(batch));
        }
    }

    /// <summary>
    /// Provide state storage in Memory
    /// </summary>
    public class InMemoryDataStore : IDataStore
    {

        public class Entity
        {
            public Entity(object value)
            {
                Date = DateTime.UtcNow;
                Value = value;
            }

            public DateTime Date { get; set; }
            public object Value { get; set; }
        }

        private readonly ConcurrentDictionary<string, Entity> _dataBag;

        public InMemoryDataStore()
        {
            _dataBag = new ConcurrentDictionary<string, Entity>();
        }

        public async Task<bool> TryAddOrUpdateAsync<TValue>(string key, TValue value) where TValue : class
        {
            return await Task.Run(() =>
            {
                Entity @out = null;
                bool result = _dataBag.TryRemove(key, out @out);

                result = _dataBag.TryAdd(key, new Entity(value));
                return result;
            });
        }

        public async Task<TValue> GetValueAsync<TValue>(string key) where TValue : class
        {
            Entity value = null;
            await Task.Run(() => _dataBag.TryGetValue(key, out value));
            if (value == null)
                return null;
            return value.Value as TValue;
        }

        public Task<IEnumerable<KeyValuePair<string, TValue>>> GetAllAsync<TValue>() where TValue : class
        {
            return Task.Run(() => _dataBag.Select(s => new KeyValuePair<string, TValue>(s.Key, s.Value.Value as TValue)));
        }

        public Task<bool> TryAddAsync<TValue>(TValue entity) where TValue : class
        {
            return Task.FromResult<bool>(true);
        }

        public async Task<bool> TryDeleteAsync(string key)
        {
            return await Task.Run(() =>
            {
                Entity value = null;
                return _dataBag.TryRemove(key, out value);
            });
        }

        public async Task TryDeleteBeforeDate(DateTimeOffset dateTimeOffset)
        {
            await Task.Run(() =>
            {
                var itemsToDelete = _dataBag.Where(d => d.Value.Date <= dateTimeOffset).ToList();

                foreach (var item in itemsToDelete)
                {
                    Entity value = null;
                    _dataBag.TryRemove(item.Key, out value);
                }
            });
        }
    }
}
