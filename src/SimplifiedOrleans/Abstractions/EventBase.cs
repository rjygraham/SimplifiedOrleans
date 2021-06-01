using Newtonsoft.Json;
using System;

namespace SimplifiedOrleans.Abstractions
{
	public class EventBase
	{
		[JsonProperty("eventTimestamp")]
		public DateTimeOffset EventTimestamp { get; set; }

		[JsonProperty("version")]
		public int Version { get; set; }
	}
}
