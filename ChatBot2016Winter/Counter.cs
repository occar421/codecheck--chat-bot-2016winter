using System;
using System.Threading;

namespace ChatBot2016Winter
{
	sealed class Counter
	{
		private static readonly Lazy<Counter> lazy = new Lazy<Counter>(() => new Counter());

		public static Counter Instance { get { return lazy.Value; } }

		public int IssueId()
		{
			return Interlocked.Increment(ref value);
		}

		private Counter() { }

		private int value = 0;
	}
}
