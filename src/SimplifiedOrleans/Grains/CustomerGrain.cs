using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orleans.EventSourcing;
using Orleans.EventSourcing.CustomStorage;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Runtime;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Events;
using SimplifiedOrleans.State;
using SimplifiedOrleans.Storage;

namespace SimplifiedOrleans.Grains
{
	[LogConsistencyProvider(ProviderName = "CustomStorage")]
	public class CustomerGrain : JournaledGrain<CustomerState, EventBase>, 
		ICustomerGrain, 
		ICustomStorageInterface<CustomerState, EventBase>
	{
		private readonly IEventStore eventStore;

		public CustomerGrain(ISiloHost siloHost)
		{
			eventStore = siloHost.Services.GetRequiredServiceByName<IEventStore>(nameof(CustomerGrain));
		}

		public override async Task OnActivateAsync()
		{
			await RefreshNow();
		}

		public async Task CreateAsync(string name)
		{
			RaiseEvent(new CustomerCreated { Name = name });
			await ConfirmEvents();
		}

		public Task<string> GetDetailsAsync()
		{
			return Task.FromResult(State.Name);
		}

		public async Task AddSensorAsync(Guid sensorId)
		{
			if (State.ActiveSensors.Contains(sensorId))
			{
				throw new InvalidOperationException("Sensor already active.");
			}

			RaiseEvent(new SensorAdded { SensorId = sensorId });
			await ConfirmEvents();
		}

		public async Task RemoveSensorAsync(Guid sensorId)
		{
			if (!State.ActiveSensors.Contains(sensorId))
			{
				throw new InvalidOperationException("Sensor not active.");
			}

			RaiseEvent(new SensorRemoved { SensorId = sensorId });
			await ConfirmEvents();
		}

		public Task<IEnumerable<Guid>> GetActiveSensors()
		{
			return Task.FromResult(State.ActiveSensors.AsEnumerable());
		}

		public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<EventBase> updates, int expectedversion)
		{
			return await eventStore.ApplyUpdatesToStorage(GrainReference.GrainIdentity.PrimaryKey.ToString(), updates, expectedversion);
		}

		public async Task<KeyValuePair<int, CustomerState>> ReadStateFromStorage()
		{
			return await eventStore.ReadStateFromStorage(GrainReference.GrainIdentity.PrimaryKey.ToString(), events =>
			{
				return new CustomerState(events);
			});
		}
	}
}
