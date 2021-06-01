using System;
using System.Collections.Generic;
using System.Linq;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Events;

namespace SimplifiedOrleans.State
{
	public class CustomerState
	{
		public string Name { get; set; }

		public HashSet<Guid> ActiveSensors { get; set; }

		public CustomerState()
		{
			ActiveSensors = new HashSet<Guid>();
		}

		public CustomerState(IOrderedEnumerable<EventBase> previousEvents)
			: this()
		{
			foreach (var previousEvent in previousEvents)
			{
				switch (previousEvent)
				{
					case CustomerCreated customerCreated:
						Apply(customerCreated);
						break;
					case SensorAdded sensorAdded:
						Apply(sensorAdded);
						break;
					case SensorRemoved sensorRemoved:
						Apply(sensorRemoved);
						break;
					default:
						break;
				}
			}
		}

		public void Apply(CustomerCreated @event)
		{
			Name = @event.Name;
		}

		public void Apply(SensorAdded @event)
		{
			ActiveSensors.Add(@event.SensorId);
		}

		public void Apply(SensorRemoved @event)
		{
			ActiveSensors.Remove(@event.SensorId);
		}
	}
}
