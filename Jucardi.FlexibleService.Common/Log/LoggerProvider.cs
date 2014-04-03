// Filename: LoggerProvider.cs
// Author:   Jucardi
// Date:     4/1/2014 7:44:41 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using log4net.Config;
using log4net.Core;

namespace Jucardi.FlexibleService.Common.Log
{
	public static class LoggerProvider
	{
		#region Constants

		private const string CONFIGURATION_SECTION = "log4net";

		#endregion

		#region Fields

		private static Dictionary<string, ILoggerEx> collection = new Dictionary<string, ILoggerEx>();

		private static WrapperMap wrapperMap = null;
		private static bool configured = false;

		#endregion

		#region Public Methods

		/// <summary>
		/// Get the logger for the specified name.
		/// </summary>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILoggerEx GetLogger(string name)
		{
			if (!configured)
				Configure();

			if (collection.ContainsKey(name))
				return collection[name];

			Log4NetLogger logger = GetLogger(Assembly.GetCallingAssembly(), name);
			collection.Add(name, logger);

			return logger;
		}

		/// <summary>
		/// Get the logger for the fully qualified name of the type specified.
		/// </summary>
		/// <param name="type">
		/// The full name of <paramref name="type"/> will
		/// be used as the name of the logger to retrieve.
		/// </param>
		/// <param name="category">The logger category.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILoggerEx GetLogger(Type type)
		{
			return GetLogger(type.FullName);
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified domain.
		/// </summary>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <param name="domain">the domain to lookup in.</param>
		/// <returns>All the defined loggers.</returns>
		public static Log4NetLogger[] GetCurrentLoggers(string domain)
		{
			return WrapLoggers(log4net.Core.LoggerManager.GetCurrentLoggers(domain));
		}

		/// <summary>
		/// Returns all the currently defined loggers in the specified assembly's domain.
		/// </summary>
		/// <remarks>
		/// The root logger is <b>not</b> included in the returned array.
		/// </remarks>
		/// <param name="assembly">the assembly to use to lookup the domain.</param>
		/// <returns>All the defined loggers.</returns>
		public static Log4NetLogger[] GetCurrentLoggers(Assembly assembly)
		{
			return WrapLoggers(log4net.Core.LoggerManager.GetCurrentLoggers(assembly));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Configures the logger manager with default settings provided in the App.Config
		/// </summary>
		/// <returns>
		/// <c>true</c> if successful.
		/// <c>false</c> if it was previously configured.
		/// </returns>
		public static bool Configure()
		{
			if (configured)
				return false;

			wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandlerEx));
			configured = true;

			return true;
		}

		/// <summary>
		/// Configures the logger manager with the given configuration stream.
		/// </summary>
		/// <param name="configuration">A serialized configuration.</param>
		/// <returns>
		/// <c>true</c> if successful.
		/// <c>false</c> if it was previously configured.
		/// </returns>
		public static bool Configure(Stream configuration)
		{
			if (configured)
				return false;

			XmlConfigurator.Configure(configuration);
			wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandlerEx));
			configured = true;

			return true;
		}

		/// <summary>
		/// Configures the logger manager with the given configuration node.
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns>
		/// <c>true</c> if successful.
		/// <c>false</c> if it was previously configured.
		/// </returns>
		public static bool Configure(XmlElement configuration)
		{
			if (configured)
				return false;

			XmlConfigurator.Configure(configuration);
			wrapperMap = new WrapperMap(new WrapperCreationHandler(WrapperCreationHandlerEx));
			configured = true;

			return true;
		}

		/// <summary>
		/// Retrieve or create a named logger.
		/// </summary>
		/// <remarks>
		/// Retrieve a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </remarks>
		/// <param name="assembly">the assembly to use to lookup the domain.</param>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static Log4NetLogger GetLogger(Assembly assembly, string name)
		{
			if (!configured)
				Configure();

			return WrapLogger(LoggerManager.GetLogger(assembly, name));
		}

		/// <summary>
		/// Lookup the wrapper object for the logger specified
		/// </summary>
		/// <param name="logger">the logger to get the wrapper for</param>
		/// <returns>The wrapper for the logger specified</returns>
		private static Log4NetLogger WrapLogger(ILogger logger)
		{
			return (Log4NetLogger)wrapperMap.GetWrapper(logger);
		}

		/// <summary>
		/// Lookup the wrapper object for the logger specified
		/// </summary>
		/// <param name="loggers">the loggers to get the wrappers for</param>
		/// <returns>>Lookup the wrapper objects for the loggers specified</returns>
		private static Log4NetLogger[] WrapLoggers(ILogger[] loggers)
		{
			Log4NetLogger[] results = new Log4NetLogger[loggers.Length];

			for (int i = 0; i < loggers.Length; i++)
				results[i] = WrapLogger(loggers[i]);

			return results;
		}

		/// <summary>
		/// Method to create the <see cref="ILoggerWrapper"/> objects used by
		/// this manager.
		/// </summary>
		/// <param name="logger">The logger to wrap</param>
		/// <returns>The wrapper for the logger specified</returns>
		private static ILoggerWrapper WrapperCreationHandlerEx(ILogger logger)
		{
			return new Log4NetLogger(logger);
		}

		#endregion
	}
}
