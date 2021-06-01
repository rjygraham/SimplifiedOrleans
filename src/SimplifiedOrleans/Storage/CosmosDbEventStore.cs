using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Configuration;

namespace SimplifiedOrleans.Storage
{
	public class CosmosDbEventStore : IEventStore
	{
		private readonly Container container;
		private readonly string queryText;

		public CosmosDbEventStore(CosmosClient cosmosClient, CosmosDbEventStoreOptions options)
		{
			var database = cosmosClient.GetDatabase(options.DatabaseId);
			container = database.GetContainer(options.ContainerId);
			queryText = $"SELECT * FROM {options.ContainerId} g where g.pk = @partitionKey";
		}

		public async Task<bool> ApplyUpdatesToStorage(string key, IReadOnlyList<EventBase> updates, int expectedversion)
		{
			var partitionKey = new PartitionKey(key);

			foreach (var update in updates)
			{
				update.EventTimestamp = DateTimeOffset.UtcNow;
				update.Version = ++expectedversion;

				var entity = new EventEnvelope
				{
					Id = Guid.NewGuid().ToString(),
					PartitionKey = key,
					Event = update
				};

				await container.CreateItemAsync(entity, partitionKey);
			}

			return true;
		}

		public async Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(string key, Func<IOrderedEnumerable<EventBase>, TState> stateFactory)
		{
			var query = new QueryDefinition(queryText).WithParameter("@partitionKey", key);

			var events = new List<EventEnvelope>();
			var iterator = container.GetItemQueryIterator<EventEnvelope>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(key) });

			while (iterator.HasMoreResults)
			{
				var response = await iterator.ReadNextAsync();
				events.AddRange(response);
			}

			var version = events.Count == 0
				? 0
				: events.Max(m => m.Event.Version);

			var state = stateFactory(events.Select(s => s.Event).OrderBy(o => o.Version));

			return new KeyValuePair<int, TState>(version, state);
		}
	}
}
