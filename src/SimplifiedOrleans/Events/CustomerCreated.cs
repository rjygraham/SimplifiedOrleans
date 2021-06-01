using Newtonsoft.Json;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Events
{
	public class CustomerCreated : EventBase
	{
		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
