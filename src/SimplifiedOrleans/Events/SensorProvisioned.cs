using System;
using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Events
{
	public class SensorProvisioned : EventBase
	{
		[JsonProperty("customerId")]
		public Guid CustomerId { get; set; }

		[JsonProperty("readingWindow")]
		public TimeSpan ReadingWindow { get; set; }
	}
}
