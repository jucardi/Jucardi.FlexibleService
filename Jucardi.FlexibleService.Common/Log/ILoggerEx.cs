// Filename: ILoggerEx.cs
// Author:   Jucardi
// Date:     4/1/2014 7:46:37 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;

namespace Jucardi.FlexibleService.Common.Log
{
	public interface ILoggerEx
	{
		/// <summary>
		/// Debug event logging.
		/// </summary>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		void Debug(string message, params object[] args);

		/// <summary>
		/// Informative event logging.
		/// </summary>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		void Info(string message, params object[] args);

		/// <summary>
		/// Warning event logging.
		/// </summary>
		/// <param name="type">The log event type.</param>
		/// <param name="message">The event message.</param>
		/// <param name="args">The message arguments.</param>
		void Warn(string message, params object[] args);

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="message">The message.</param>
		/// <param name="args">The message arguments.</param>
		void Error(string message, params object[] args);

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception">The exception.</param>
		void Error(Exception exception);

		/// <summary>
		/// Error level event logging.
		/// </summary>
		/// <param name="exception">The exception.</param>
		/// <param name="message">The message.</param>
		/// <param name="args">The message arguments.</param>
		void Error(Exception exception, string message, params object[] args);
	}
}
