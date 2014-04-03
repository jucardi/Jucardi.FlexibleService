// Filename: DataWorker.cs
// Author:   wolKlow
// Date:     4/1/2014 8:51:53 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections.Generic;
using System.Threading;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService.Common.BaseWorkers
{
	public abstract class DataWorker<T> : TimerWorker
	{
		#region Fields

		private int  _maxProcessThreads = 1;
		private int  _maxWaitingThreads = 1;
		private bool _stopRequested     = false;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when the data process is stopped. Returns the last data processed.
		/// </summary>
		protected event Action<T> OnStopProcessData;

		#endregion

		#region Logger

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		protected static ILoggerEx Logger
		{
			get { return LoggerProvider.GetLogger(typeof(DataWorker<T>)); }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the maximum process threads.
		/// </summary>
		/// <value>
		/// The maximum process threads.
		/// </value>
		public int MaxProcessThreads
		{
			get { return _maxProcessThreads; }
			set { _maxProcessThreads = value; }
		}

		/// <summary>
		/// Gets or sets the maximum waiting threads.
		/// </summary>
		/// <value>
		/// The maximum waiting threads.
		/// </value>
		public int MaxWaitingThreads
		{
			get { return _maxWaitingThreads; }
			set { _maxWaitingThreads = value; }
		}

		#endregion

		/// <summary>
		/// Initializes this instance before the start thread.
		/// </summary>
		protected override void Initialize()
		{
			_stopRequested = false;
			Logger.Info("MaxProcessThreads {0} MaxWaitingThreads {1}", MaxProcessThreads, MaxWaitingThreads);
			base.Initialize();
		}

		/// <summary>
		/// Called when then timer with the specified interval ticks.
		/// </summary>
		protected override void OnIntervalTick()
		{
			int            records         = 0;
			IEnumerable<T> dataCollections = GetData();

			foreach (T data in dataCollections)
			{
				records++;
				this.ProcessData(data);

				if (!_stopRequested)
					continue;

				if (OnStopProcessData != null)
					OnStopProcessData(data);

				return;
			}
		}

		/// <summary>
		/// Called when a stop request has been made.
		/// </summary>
		protected override void OnStop()
		{
			_stopRequested = true;
			base.OnStop();
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<T> GetData();

		/// <summary>
		/// Processes the data.
		/// </summary>
		/// <param name="data">The data.</param>
		protected abstract void ProcessData(T data);
	}
}
