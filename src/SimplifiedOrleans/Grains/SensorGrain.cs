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
	public class SensorGrain : JournaledGrain<SensorState, EventBase>,
		ISensorGrain,
		ICustomStorageInterface<SensorState, EventBase>
	{
		private readonly IEventStore eventStore;

		public SensorGrain(ISiloHost siloHost)
		{
			eventStore = siloHost.Services.GetRequiredServiceByName<IEventStore>(nameof(SensorGrain));
		}

		public async Task ProvisionAsync(ISensorConfiguration configuration)
		{
			var customer = GrainFactory.GetGrain<ICustomerGrain>(configuration.CustomerId);
			await customer.AddSensorAsync(GrainReference.GrainIdentity.PrimaryKey);

			RaiseEvent(new SensorProvisioned { CustomerId = configuration.CustomerId, ReadingWindow = configuration.ReadingWindow });
			await ConfirmEvents();
		}
		public async Task DeprovisionAsync()
		{
			var customer = GrainFactory.GetGrain<ICustomerGrain>(State.Configuration.CustomerId);
			await customer.RemoveSensorAsync(GrainReference.GrainIdentity.PrimaryKey);

			RaiseEvent(new SensorDeprovisioned());
			await ConfirmEvents();
		}

		public Task<ISensorConfiguration> GetConfiguration()
		{
			return Task.FromResult(State.Configuration);
		}

		public async Task RecordReadingAsync(ISensorReading reading)
		{
			if (State.CurrentReading is null || reading.Timestamp > State.CurrentReading.Timestamp)
			{
				RaiseEvent(new ReadingRecorded { ScanType = reading.ScanType, Type = reading.Type,  Timestamp = reading.Timestamp, Value = reading.Value });
				await ConfirmEvents();
			}
		}

		public Task<ISensorReading> GetCurrentReadingAsync()
		{
			return Task.FromResult(State.CurrentReading);
		}

		public async Task<IEnumerable<ISensorReading>> GetReadingsAfter(DateTimeOffset timestamp)
		{
			if (State.Readings.Count > 0)
			{
				return State.Readings.Where(w => w.Timestamp > timestamp).ToList();
			}

			return new HashSet<ISensorReading>();
		}

		public async Task<bool> ApplyUpdatesToStorage(IReadOnlyList<EventBase> updates, int expectedversion)
		{
			return await eventStore.ApplyUpdatesToStorage(GrainReference.GrainIdentity.PrimaryKey.ToString(), updates, expectedversion);
		}

		public async Task<KeyValuePair<int, SensorState>> ReadStateFromStorage()
		{
			return await eventStore.ReadStateFromStorage(GrainReference.GrainIdentity.PrimaryKey.ToString(), events =>
			{
				return new SensorState(events);
			});
		}
	}
}
