using System;

namespace SimplifiedOrleans.Abstractions
{
	public interface ISensorConfiguration
	{
		Guid CustomerId { get; init; }
		TimeSpan ReadingWindow { get; init; }
		bool IsProvisioned { get; set; }
	}
}
