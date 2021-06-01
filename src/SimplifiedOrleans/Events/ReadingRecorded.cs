using System;
using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Events
{
	public class ReadingRecorded : EventBase
	{
		[JsonProperty("Timestamp")]
		public DateTimeOffset Timestamp { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("scanType")]
		public string ScanType { get; set; }

		[JsonProperty("value")]
		public int Value { get; set; }
	}
}
