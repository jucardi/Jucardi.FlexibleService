// Filename: Throttle.cs
// Author:   wolKlow
// Date:     3/28/2014 5:33:37 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Threading;

namespace Jucardi.FlexibleService.Common
{
	public class Throttle
	{
		#region Fields

		private readonly object   _syncRoot = new object();
		private readonly TimeSpan _interval;
		private readonly int      _limit    = 0;
		private DateTime          _next     = DateTime.MinValue;
		private int               _current  = 0;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Throttle"/> class.
		/// </summary>
		/// <param name="interval">The interval.</param>
		/// <param name="limit">The limit.</param>
		public Throttle(TimeSpan interval, int limit)
		{
			_interval = interval;
			_limit    = limit;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the disabled throttle.
		/// </summary>
		/// <value>
		/// The disabled throttle.
		/// </value>
		public static Throttle DisabledThrottle
		{
			get
			{
				return new Throttle(new TimeSpan(0, 0, 0), 0);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Waits for the next message to be dispatched.
		/// </summary>
		public void Wait()
		{
			if (_limit == 0)
				return;

			lock (_syncRoot)
			{
				DateTime now = DateTime.UtcNow;

				if (_next < now)
				{
					_current = 0;
					_next = now + _interval;
				}

				_current++;

				if (_current < _limit)
					return;

				Thread.Sleep(_next - now);
			}
		}

		#endregion
	}
}

