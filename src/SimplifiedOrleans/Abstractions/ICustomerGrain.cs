using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace SimplifiedOrleans.Abstractions
{
	public interface ICustomerGrain : IGrainWithGuidKey
	{
		Task CreateAsync(string name);
		Task<string> GetDetailsAsync();
		Task<IEnumerable<Guid>> GetActiveSensors();
		Task AddSensorAsync(Guid sensorId);
		Task RemoveSensorAsync(Guid sensorId);
	}
}
