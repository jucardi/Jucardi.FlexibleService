// Filename: WorkerProcess.cs
// Author:   Jucardi
// Date:     3/26/2014 1:40:45 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jucardi.FlexibleService.Common;
using Jucardi.FlexibleService.Common.Collections;
using Jucardi.FlexibleService.Common.Extensions;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService
{
	[Serializable]
	internal class WorkerProcess
	{
		#region Fields

		private List<IWorker> _workerList = new List<IWorker>();

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
			get { return LoggerProvider.GetLogger(typeof(WorkerProcess)); }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Executes the assembly.
		/// </summary>
		/// <param name="file">The file path.</param>
		/// <param name="klass">The class.</param>
		public void ExecuteWorker(string file, string klass)
		{
			IWorker worker = CreateWorker(file, klass);
			ConfigureCommonValues(worker);
			ExecuteWorker(worker);
		}

		/// <summary>
		/// Executes the worker.
		/// </summary>
		/// <param name="info">The worker information.</param>
		public void ExecuteWorker(ExtensionInfo info)
		{
			Logger.Info("Creating worker with configuration set {0}", info.Name);
			IWorker worker = CreateWorker(info.AssemblyPath, info.ClassName);
			ConfigureCommonValues(worker);
			info.Configure(worker);
			ExecuteWorker(worker);
		}

		/// <summary>
		/// Executes the worker.
		/// </summary>
		/// <param name="worker">The worker.</param>
		private void ExecuteWorker(IWorker worker)
		{
			Logger.Info("Begining execution of worker {0}", worker.Name);
			_workerList.Add(worker);
			worker.Start();
		}

		/// <summary>
		/// Creates the worker.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="klass">The klass.</param>
		private IWorker CreateWorker(string file, string klass)
		{
			byte[]   assemblyRawData = File.ReadAllBytes(file);
			Assembly assembly        = Assembly.Load(assemblyRawData);
			Type     type            = assembly.GetType(klass);

			return (IWorker)Activator.CreateInstance(type);
		}

		/// <summary>
		/// Stops the workers.
		/// </summary>
		public void StopWorkers()
		{
			foreach (IWorker worker in _workerList)
			{
				Logger.Info("Stopping worker {0}", worker.Name);
				worker.Stop();
			}
		}

		/// <summary>
		/// Configures the common values.
		/// </summary>
		/// <param name="obj">The object.</param>
		private void ConfigureCommonValues(object obj)
		{
			foreach (NameValue nameValue in FlexibleServiceConfiguration.Configuration.CommonProperties)
			{
				try
				{
					nameValue.AssignTo(obj);
				}
				catch (Exception ex)
				{
					Logger.Warn(ex.Message);
				}
			}
		}

		#endregion
	}
}
