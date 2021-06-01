using System;
using Newtonsoft.Json;

namespace SimplifiedOrleans.Models
{
	public class CustomerModel
	{
		[JsonProperty("id")]
		public Guid? Id { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }
	}
}
