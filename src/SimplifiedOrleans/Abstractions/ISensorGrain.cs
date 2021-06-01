using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace SimplifiedOrleans.Abstractions
{
	public interface ISensorGrain : IGrainWithGuidKey
	{
		Task ProvisionAsync(ISensorConfiguration configuration);
		Task RecordReadingAsync(ISensorReading reading);
		Task<ISensorConfiguration> GetConfiguration();
		Task<ISensorReading> GetCurrentReadingAsync();
		Task<IEnumerable<ISensorReading>> GetReadingsAfter(DateTimeOffset timestamp);
		Task DeprovisionAsync();
	}
}
