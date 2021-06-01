using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimplifiedOrleans.Abstractions;

namespace SimplifiedOrleans.Storage
{
	public interface IEventStore
	{
		Task<bool> ApplyUpdatesToStorage(string key, IReadOnlyList<EventBase> updates, int expectedversion);
		Task<KeyValuePair<int, TState>> ReadStateFromStorage<TState>(string key, Func<IOrderedEnumerable<EventBase>, TState> stateFactory);
	}
}
