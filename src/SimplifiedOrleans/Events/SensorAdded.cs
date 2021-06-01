using System;
using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Events
{
	public class SensorAdded : EventBase
	{
		[JsonProperty("sensorId")]
		public Guid SensorId { get; set; }
	}
}
