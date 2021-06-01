using System;
using Newtonsoft.Json;

namespace SimplifiedOrleans.Models
{
	public class SensorConfigurationModel
	{
		[JsonProperty("id")]
		public Guid? Id { get; set; }

		[JsonProperty("customerId")]
		public Guid CustomerId { get; set; }

		[JsonProperty("readingWindow")]
		public TimeSpan ReadingWindow { get; set; }

		[JsonProperty("isProvisioned")]
		public bool? IsProvisioned { get; set; }
	}
}
