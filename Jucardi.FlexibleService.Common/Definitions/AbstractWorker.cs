// Filename: AbstractWorker.cs
// Author:   Jucardi
// Date:     3/26/2014 2:41:51 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Threading;

namespace Jucardi.FlexibleService.Common
{
	public abstract class AbstractWorker : IWorker
	{
		#region Fields

		private Thread _executionThread  = null;
		private int    _stopTimeout      = 5000;

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether this instance is running.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is running; otherwise, <c>false</c>.
		/// </value>
		public virtual bool IsRunning
		{
			get { return _executionThread != null && _executionThread.IsAlive; }
		}

		/// <summary>
		/// Gets or sets the stop timeout.
		/// (Max amount of ms to wait for a worker to stop once the stop call has been made)
		/// </summary>
		/// <value>
		/// The stop timeout.
		/// </value>
		public int StopTimeout
		{
			get { return _stopTimeout; }
			set { _stopTimeout = value; }
		}

		/// <summary>
		/// Gets or sets the worker name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
		public abstract string Name { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Starts the execution of the worker.
		/// </summary>
		public void Start()
		{
			if (_executionThread != null && _executionThread.IsAlive)
				_executionThread.Abort();

			this.Initialize();

			_executionThread = new Thread(new ThreadStart(this.StartThread));
			_executionThread.IsBackground = true;
			_executionThread.Start();
		}

		/// <summary>
		/// Stops the execution of the worker.
		/// </summary>
		public void Stop()
		{
			if (!this.IsRunning)
				return;

			Thread stopThread = new Thread(new ThreadStart(this.StopThread));
			stopThread.Start();

			if (stopThread.Join(StopTimeout))
				return;

			_executionThread.Abort();
			_executionThread = null;
			GC.Collect();
		}

		/// <summary>
		/// Initializes this instance before the start thread.
		/// </summary>
		protected virtual void Initialize()
		{
		}

		/// <summary>
		/// Execution method of the worker, which will be ran in a different thread when Start is invoked.
		/// </summary>
		protected abstract void StartThread();

		/// <summary>
		/// Execution of the stop process of the worker, which will be ran in a different thread when Stop is invoked.
		/// </summary>
		protected abstract void StopThread();

		#endregion
	}
}
