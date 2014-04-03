// Filename: QueueWorker.cs
// Author:   wolKlow
// Date:     3/28/2014 5:09:56 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Messaging;
using System.Threading;
using Jucardi.FlexibleService.Common.Exceptions;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService.Common.BaseWorkers
{
	public abstract class QueueWorker : TimerWorker
	{
		#region Constants

		private static readonly TimeSpan READ_QUEUE_TIMEOUT = new TimeSpan(0, 0, 1);

		#endregion

		#region Fields

		private int          _throttleLimit      = 0;
		private string       _queuePath          = null;
		private MessageQueue _queue              = null;
		private Throttle     _throttle           = null;

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
			get { return LoggerProvider.GetLogger(typeof(QueueWorker)); }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the queue path.
		/// </summary>
		/// <value>
		/// The queue path.
		/// </value>
		public string QueuePath
		{
			get { return _queuePath; }
			set { _queuePath = value; }
		}

		/// <summary>
		/// Gets or sets the synchronize timeout.
		/// </summary>
		/// <value>
		/// The synchronize timeout.
		/// </value>
		public int ThrottleLimit
		{
			get { return _throttleLimit; }
			set { _throttleLimit = value; }
		}

		/// <summary>
		/// Gets or sets the queue.
		/// </summary>
		/// <value>
		/// The queue.
		/// </value>
		protected MessageQueue Queue
		{
			get { return _queue; }
			set { _queue = value; }
		}

		/// <summary>
		/// Gets the message types.
		/// </summary>
		/// <value>
		/// The message types.
		/// </value>
		protected abstract Type[] MessageTypes { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Initializes this instance before the start thread.
		/// </summary>
		protected override void Initialize()
		{
			if (string.IsNullOrEmpty(QueuePath))
				throw new WorkerException("QueuePath", Name);

			_throttle = ThrottleLimit > 0 ? new Throttle(new TimeSpan(0, 0, 1), ThrottleLimit) : Throttle.DisabledThrottle;

			this.Queue = new MessageQueue(QueuePath)
			{
				Formatter = new XmlMessageFormatter
				{
					TargetTypes = MessageTypes
				}
			};

			Logger.Info("Queue " + QueuePath);
			Logger.Info("Queue can read {0}", Queue.CanRead);

			base.Initialize();
		}

		/// <summary>
		/// Called when then timer with the specified interval ticks.
		/// </summary>
		protected override void OnIntervalTick()
		{
			if (ReadQueueMessage())
				_throttle.Wait();
		}

		/// <summary>
		/// Reads a message from the queue.
		/// </summary>
		/// <returns></returns>
		private bool ReadQueueMessage()
		{
			try
			{
				Message msg = Queue.Receive(READ_QUEUE_TIMEOUT);

				if (msg != null)
				{
					ThreadPool.QueueUserWorkItem(ReceiveMessageCallBack, msg);
					return true;
				}
			}
			catch (MessageQueueException ex)
			{
				if (ex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout)
					return false;

				
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}

			return false;
		}

		/// <summary>
		/// Receives the message call back.
		/// </summary>
		/// <param name="state">The state.</param>
		private void ReceiveMessageCallBack(object state)
		{
			try
			{
				OnMessageReceived((Message)state);
			}
			catch (Exception ex)
			{
				Logger.Error(ex);
			}
		}

		/// <summary>
		/// Called when a message is received.
		/// </summary>
		/// <param name="message">The message.</param>
		protected abstract void OnMessageReceived(Message message);

		#endregion
	}
}
