// Filename: Log4NetLogger.cs
// Author:   Jucardi
// Date:     4/1/2014 7:44:11 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Globalization;
using log4net.Core;
using System.Collections.Generic;

namespace Jucardi.FlexibleService.Common.Log
{
	public sealed class  Log4NetLogger : ILoggerEx, ILoggerWrapper
	{
		#region Constants

		private static readonly Type   THIS_DECLARING_TYPE = typeof(Log4NetLogger);
		private static readonly Level  DEBUG_LEVEL         = new Level(50000, "Debug");
		private static readonly Level  INFO_LEVEL          = new Level(50100, "Info ");
		private static readonly Level  WARN_LEVEL          = new Level(50200, "Warn ");
		private static readonly Level  ERROR_LEVEL         = new Level(50300, "Error");

		#endregion

		#region Fields

		private readonly ILogger logger = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Log4NetLogger"/> class.
		/// </summary>
		/// <param name="logger">The log4net logger object.</param>
		public Log4NetLogger(ILogger logger)
		{
			this.logger = logger;

			LevelMap map = logger.Repository.LevelMap;

			map.LookupWithDefault(DEBUG_LEVEL);
			map.LookupWithDefault(INFO_LEVEL);
			map.LookupWithDefault(WARN_LEVEL);
			map.LookupWithDefault(ERROR_LEVEL);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the Log4Net logger.
		/// </summary>
		/// <value>
		/// The Log4Net logger.
		/// </value>
		public ILogger Logger
		{
			get { return this.logger; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Debug event logging.
		/// </summary>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		public void Debug(string message, params object[] args)
		{
			this.Log(DEBUG_LEVEL, null, 0, message, args);
		}

		/// <summary>
		/// Informative event logging.
		/// </summary>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		public void Info(string message, params object[] args)
		{
			this.Log(INFO_LEVEL, null, 0, message, args);
		}

		/// <summary>
		/// Warning event logging.
		/// </summary>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		public void Warn(string message, params object[] args)
		{
			this.Log(WARN_LEVEL, null, 0, message, args);
		}

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="code">The code.</param>
		public void Error(Exception exception, Enum code)
		{
			this.Log(ERROR_LEVEL, exception, Convert.ToInt32(code), null, null);
		}

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="code">The code.</param>
		public void Error(Exception exception, int code)
		{
			this.Log(ERROR_LEVEL, exception, code, null, null);
		}

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The message arguments.</param>
		public void Error(string message, params object[] args)
		{
			this.Log(ERROR_LEVEL, null, 0, message, args);
		}

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception">The exception.</param>
		public void Error(Exception exception)
		{
			this.Log(ERROR_LEVEL, exception, 0, null, null);
		}

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception"></param>
		/// <param name="message">The message.</param>
		/// <param name="args">The message arguments.</param>
		public void Error(Exception exception, string message, params object[] args)
		{
			this.Log(ERROR_LEVEL, exception, 0, message, args);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Logs the specified message.
		/// </summary>
		/// <param name="type">The log event type.</param>
		/// <param name="level">The logger level.</param>
		/// <param name="exception">The error exception.</param>
		/// <param name="code">The event code.</param>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message args.</param>
		private void Log(Level level, Exception exception, int code, string message, params object[] args)
		{
			if (!this.logger.IsEnabledFor(level))
				return;

			string logMessage = string.IsNullOrEmpty(message) ? string.Empty : string.Format(CultureInfo.InvariantCulture, message, args);

			LoggingEvent loggingEvent = new LoggingEvent(THIS_DECLARING_TYPE, this.logger.Repository, this.logger.Name, level, logMessage, exception);

			foreach (KeyValuePair<string, string> property in LoggerContext.Current.GlobalProperties)
				loggingEvent.Properties[property.Key] = property.Value ?? string.Empty;

			foreach (KeyValuePair<string, string> property in LoggerContext.Current.ThreadProperties)
				loggingEvent.Properties[property.Key] = property.Value ?? string.Empty;

			this.logger.Log(loggingEvent);
		}

		#endregion
	}
}
