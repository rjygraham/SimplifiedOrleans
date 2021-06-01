using System;

namespace SimplifiedOrleans.Abstractions
{
	public interface ISensorReading
	{
		DateTimeOffset Timestamp { get; init; }
		string Type { get; init; }
		string ScanType { get; init; }
		int Value { get; init; }
	}
}
