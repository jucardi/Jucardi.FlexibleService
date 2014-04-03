// Filename: LoggerContext.cs
// Author:   Jucardi
// Date:     4/1/2014 7:49:03 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections.Generic;

namespace Jucardi.FlexibleService.Common.Log
{
	/// <summary>
	/// Provides access to the logger context.
	/// </summary>
	public sealed class LoggerContext
	{
		#region Fields

		[ThreadStatic]
		private static LoggerContext              _currentContext   = null;
		private static Dictionary<string, string> _globalProperties = new Dictionary<string, string>();

		private Dictionary<string, string> threadProperties = new Dictionary<string, string>();

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current the thread context.
		/// </summary>
		public static LoggerContext Current
		{
			get
			{
				if (_currentContext == null)
					_currentContext = new LoggerContext();

				return _currentContext;
			}
		}

		/// <summary>
		/// Gets the logger events custom properties dictionary independent for every thread.
		/// </summary>
		public IDictionary<string, string> ThreadProperties
		{
			get { return this.threadProperties; }
		}

		/// <summary>
		/// Gets the logger events custom properties dictionary.
		/// </summary>
		public IDictionary<string, string> GlobalProperties
		{
			get { return _globalProperties; }
		}

		#endregion
	}
}
