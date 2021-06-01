using System;
using Newtonsoft.Json;

namespace SimplifiedOrleans.Models
{
	public class ReadingModel
	{
		[JsonProperty("id")]
		public Guid Id { get; set; }

		[JsonProperty("ts")]
		public long Ts { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("scanType")]
		public string ScanType { get; set; }

		[JsonProperty("value")]
		public int Value { get; set; }
	}
}
