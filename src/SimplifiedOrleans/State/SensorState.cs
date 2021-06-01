using System;
using System.Collections.Generic;
using System.Linq;
using SimplifiedOrleans.Abstractions;
using SimplifiedOrleans.Entities;
using SimplifiedOrleans.Events;

namespace SimplifiedOrleans.State
{
	public class SensorState
	{
		public ISensorConfiguration Configuration { get; set; }
		public ISensorReading CurrentReading { get; set; }
		public Queue<ISensorReading> Readings { get; set; }

		public SensorState()
		{
			Readings = new Queue<ISensorReading>();
		}

		public SensorState(IOrderedEnumerable<EventBase> previousEvents)
			: this()
		{
			foreach (var previousEvent in previousEvents)
			{
				switch (previousEvent)
				{
					case SensorProvisioned sensorProvisioned:
						Apply(sensorProvisioned);
						break;
					case ReadingRecorded readingRecorded:
						Apply(readingRecorded);
						break;
					case SensorDeprovisioned sensorDeprovisioned:
						Apply(sensorDeprovisioned);
						break;
					default:
						break;
				}
			}
		}

		public void Apply(SensorProvisioned @event)
		{
			Configuration = new SensorConfiguration { CustomerId = @event.CustomerId, ReadingWindow = @event.ReadingWindow, IsProvisioned = true };
		}

		public void Apply(ReadingRecorded @event)
		{
			var windowEdge = DateTimeOffset.UtcNow.AddSeconds(Configuration.ReadingWindow.TotalSeconds * -1);

			while (Readings.Count > 0 && Readings.Peek().Timestamp < windowEdge)
			{
				Readings.Dequeue();
			}

			if ((CurrentReading == null || @event.Timestamp > CurrentReading.Timestamp))
			{
				var current = new SensorReading { Type = @event.Type, ScanType = @event.ScanType, Timestamp = @event.Timestamp, Value = @event.Value };
				CurrentReading = current;

				if (@event.Timestamp > windowEdge)
				{
					Readings.Enqueue(current);
				}
			}
		}

		public void Apply(SensorDeprovisioned @event)
		{
			Configuration.IsProvisioned = false;
		}
	}
}
