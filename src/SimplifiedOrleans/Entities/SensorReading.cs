using System;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Entities
{
	public class SensorReading : ISensorReading
	{
		public DateTimeOffset Timestamp { get; init; }
		public string Type { get; init; }
		public string ScanType { get; init; }
		public int Value { get; init; }
	}
}
