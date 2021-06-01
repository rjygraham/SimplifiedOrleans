using System;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Entities
{
	public class SensorConfiguration : ISensorConfiguration
	{
		public Guid CustomerId { get; init; }
		public TimeSpan ReadingWindow { get; init; }
		public bool IsProvisioned { get; set; }
	}
}
