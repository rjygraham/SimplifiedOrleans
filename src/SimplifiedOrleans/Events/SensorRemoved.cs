using System;
using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Events
{
	public class SensorRemoved : EventBase
	{
		[JsonProperty("sensorId")]
		public Guid SensorId { get; set; }
	}
}
