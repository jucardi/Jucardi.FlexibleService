// Filename: TimerWorker.cs
// Author:   Jucardi
// Date:     3/28/2014 4:17:26 PM
//
// The use of this code is subject to Open Source License Agreement.

using System.Threading;

namespace Jucardi.FlexibleService.Common.BaseWorkers
{
	public abstract class TimerWorker : AbstractWorker
	{
		#region Fields

		private int  _interval = 500;
		private bool _isRunning;

		#endregion

		#region Constructor

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the interval in milliseconds.
		/// </summary>
		/// <value>
		/// The interval.
		/// </value>
		public int Interval
		{
			get { return _interval; }
			set { _interval = value; }
		}

		#endregion

		#region Event handlers

		/// <summary>
		/// Called when then timer with the specified interval ticks.
		/// </summary>
		protected abstract void OnIntervalTick();

		#endregion

		#region Methods

		/// <summary>
		/// Execution method of the worker, which will be ran in a different thread when Start is invoked.
		/// </summary>
		protected override void StartThread()
		{
			_isRunning = true;

			while (_isRunning)
			{
				this.OnIntervalTick();
				Thread.Sleep(this.Interval);
			}
		}

		/// <summary>
		/// Execution of the stop process of the worker, which will be ran in a different thread when Stop is invoked.
		/// </summary>
		protected override void StopThread()
		{
			_isRunning = false;
			this.OnStop();
		}

		/// <summary>
		/// Called when a stop request has been made.
		/// </summary>
		protected virtual void OnStop()
		{
		}

		#endregion
	}
}
