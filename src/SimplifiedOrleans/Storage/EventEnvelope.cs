using System;
using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Storage
{
	[Serializable]
	public class EventEnvelope
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("pk")]
		public string PartitionKey { get; set; }

		[JsonProperty("event")]
		public EventBase Event { get; set; }
	}
}
