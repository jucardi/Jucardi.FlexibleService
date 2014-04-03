// Filename: WorkerFactory.cs
// Author:   Jucardi
// Date:     3/24/2014 10:14:18 AM
//
// The use of this code is subject to Open Source License Agreement.

using System.Runtime.CompilerServices;
using System.Threading;
using Jucardi.FlexibleService.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService
{
	public static class WorkerFactory
	{
		#region Constants

		public const string DEFAULT_PATH = @".\Extensions";

		#endregion

		#region Fields

		private static List<string>                      _assemblyDirectories   = new List<string>();
		private static FileSystemWatcher                 _assemblyWatcher       = new FileSystemWatcher();
		private static FileSystemWatcher                 _configurationWatcher  = new FileSystemWatcher();
		private static Dictionary<string, AppDomain>     _executingDomains      = new Dictionary<string, AppDomain>();
		private static Dictionary<string, WorkerProcess> _executingProcesses    = new Dictionary<string, WorkerProcess>();
		private static bool                              _initialized           = false;

		#endregion

		#region Logger

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		private static ILoggerEx Logger
		{
			get { return LoggerProvider.GetLogger(typeof(WorkerFactory)); }
		}

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes the <see cref="WorkerFactory"/> class.
		/// </summary>
		static WorkerFactory()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnAssemblyResolve);
			_assemblyDirectories.Add(DEFAULT_PATH);
			_assemblyDirectories.Add(AppDomain.CurrentDomain.BaseDirectory);
			_assemblyDirectories.Add(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

			//_configuration.Load(ConfigurationManager.GetSection(WORKERS_CONFIG_ELEMENT_NAME));
		}

		#endregion

		#region Event Handlers

		/// <summary>
		/// Called when assembly resolve is required.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.ResolveEventArgs"/> instance containing the event data.</param>
		/// <returns>The assembly object.</returns>
		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string name = string.Format(CultureInfo.InvariantCulture, "{0}.dll", args.Name.Split(',')[0]);

			// Resolve extension assemblies dependencies path.
			foreach (string directory in _assemblyDirectories)
			{
				string path = Path.GetFullPath(Path.Combine(directory, name));

				if (File.Exists(path))
					return Assembly.LoadFile(path);
			}

			return null;
		}

		/// <summary>
		/// Handles the Renamed event of the _assemblyWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="RenamedEventArgs"/> instance containing the event data.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private static void OnRenamed(object sender, RenamedEventArgs e)
		{
			StopAssembly(e.OldFullPath);
			ReloadAssembly(e.FullPath);
		}

		/// <summary>
		/// Handles the Changed event of the _assemblyWatcher control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="FileSystemEventArgs" /> instance containing the event data.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private static void OnChanged(object sender, FileSystemEventArgs e)
		{
			StopAssembly(e.FullPath);
			ReloadAssembly(e.FullPath);
		}

		/// <summary>
		/// Called when [configuration changed].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="FileSystemEventArgs"/> instance containing the event data.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		private static void OnConfigurationChanged(object sender, FileSystemEventArgs e)
		{
			Stop();
			Thread.Sleep(500);
			FlexibleServiceConfiguration.Initialize();
			Start();
		}

		/// <summary>
		/// Handles the UnhandledException event of the domain control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="UnhandledExceptionEventArgs"/> instance containing the event data.</param>
		private static void OnWorkerDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			AppDomain domain = sender as AppDomain;

			foreach (KeyValuePair<string, AppDomain> item in _executingDomains)
			{
				if (!domain.Equals(item.Value))
					continue;

				try
				{
					StopAssembly(item.Key);
				}
				catch
				{
				}

				Thread.Sleep(30000);
				ReloadAssembly(item.Key);
				return;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts the execution of the workers
		/// </summary>
		public static void Start()
		{
			Initialize();

			if (FlexibleServiceConfiguration.Configuration.Types == null)
				return;

			foreach (ExtensionInfo info in FlexibleServiceConfiguration.Configuration.Types)
				StartWorker(info);
		}

		/// <summary>
		/// Stops the execution of all workers
		/// </summary>
		public static void Stop()
		{
			foreach (KeyValuePair<string, WorkerProcess> executingProcess in _executingProcesses)
				executingProcess.Value.StopWorkers();

			foreach (KeyValuePair<string, AppDomain> domain in _executingDomains)
				UnloadDomain(domain.Value);

			_executingProcesses.Clear();
			_executingDomains.Clear();
		}

		/// <summary>
		/// Starts the assembly.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <returns>
		///   <c>true</c> if the worker was successfully started, otherwise <c>false</c>
		/// </returns>
		public static bool StartWorker(ExtensionInfo info)
		{
			Logger.Info("Starting worker with name: {0}", info.Name);

			if (!File.Exists(Path.GetFullPath(info.AssemblyPath)))
			{
				Logger.Warn("Unable to start worker '{0}', the assembly {1} could not be found.", info.Name, Path.GetFileName(info.AssemblyPath));
				return false;
			}

			WorkerProcess process = null;

			if (_executingProcesses.ContainsKey(Path.GetFullPath(info.AssemblyPath)))
			{
				Logger.Debug("Executing domain for assembly '{0}' found, executing worker '{1}' in existing domain.", Path.GetFileName(info.AssemblyPath), info.Name);
				process = _executingProcesses[info.AssemblyPath];
			}
			else
			{
				Logger.Debug("Creating domain for assembly '{0}'", info.AssemblyPath);
				Type           remoteType = typeof(WorkerProcess);
				AppDomainSetup domainInfo = new AppDomainSetup();

				// Prepares a different domain for extension assemblies search. This isolates the loaded assemblies from the application domain.
				domainInfo.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

				AppDomain     domain  = AppDomain.CreateDomain(Guid.NewGuid().ToString().GetHashCode().ToString("x"), null, domainInfo);
				process = (WorkerProcess)domain.CreateInstanceAndUnwrap(remoteType.Assembly.FullName, remoteType.FullName);

				domain.UnhandledException += OnWorkerDomainUnhandledException;
				_executingDomains[Path.GetFullPath(info.AssemblyPath)]   = domain;
				_executingProcesses[Path.GetFullPath(info.AssemblyPath)] = process;
			}

			process.ExecuteWorker(info);
			return true;
		}

		/// <summary>
		/// Reloads the assembly.
		/// </summary>
		/// <param name="path">The path.</param>
		private static void ReloadAssembly(string path)
		{
			Logger.Info("Reloading assembly {0}", path);

			foreach (ExtensionInfo info in FlexibleServiceConfiguration.Configuration.Types)
			{
				if (string.Equals(Path.GetFullPath(info.AssemblyPath), Path.GetFullPath(path), StringComparison.InvariantCultureIgnoreCase))
					StartWorker(info);
			}
		}

		/// <summary>
		/// Stops the assembly.
		/// </summary>
		/// <param name="file">The file.</param>
		private static void StopAssembly(string file)
		{
			Logger.Info("Stopping the execution of the assembly {0}", Path.GetFileName(file));

			if (_executingProcesses.ContainsKey(file))
			{
				_executingProcesses[file].StopWorkers();
				_executingProcesses.Remove(file);
			}

			if (_executingDomains.ContainsKey(file))
			{
				UnloadDomain(_executingDomains[file]);
				_executingDomains.Remove(file);
			}
		}

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		public static void Initialize()
		{
			if (_initialized)
				return;

			_assemblyWatcher.Path   = Path.GetFullPath(DEFAULT_PATH);
			_assemblyWatcher.Filter = "*.dll";

			_assemblyWatcher.Created += OnChanged;
			_assemblyWatcher.Changed += OnChanged;
			_assemblyWatcher.Deleted += OnChanged;
			_assemblyWatcher.Renamed += OnRenamed;

			_configurationWatcher.Path   = AppDomain.CurrentDomain.BaseDirectory;
			_configurationWatcher.Filter = FlexibleServiceConfiguration.CONFIGURATION_FILE;

			_configurationWatcher.Created += OnConfigurationChanged;
			_configurationWatcher.Changed += OnConfigurationChanged;
			_configurationWatcher.Deleted += OnConfigurationChanged;
			_configurationWatcher.Renamed += OnConfigurationChanged;

			_assemblyWatcher.EnableRaisingEvents      = true;
			_configurationWatcher.EnableRaisingEvents = true;

			_initialized = true;
		}

		/// <summary>
		/// Unloads the domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		private static void UnloadDomain(AppDomain domain)
		{
			Thread unloadThread = new Thread(new ParameterizedThreadStart(UnloadDomain));
			unloadThread.Start(domain);
		}

		/// <summary>
		/// Unloads the domain.
		/// </summary>
		/// <param name="domain">The domain.</param>
		private static void UnloadDomain(object obj)
		{
			AppDomain domain = obj as AppDomain;
			AppDomain.Unload(domain);
		}

		#endregion
	}
}
