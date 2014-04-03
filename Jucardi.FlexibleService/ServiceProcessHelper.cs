// Filename: ServiceProcessHelper.cs
// Author:   Jucardi
// Date:     3/23/2014 11:31:05 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Reflection;
using System.Collections;
using System.ServiceProcess;
using System.Configuration.Install;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService
{
	/// <summary>
	/// Service process architecture helper.
	/// </summary>
	public static class ServiceProcessHelper
	{
		#region Constants

		private static readonly TimeSpan WAIT_TIMEOUT = new TimeSpan(0, 5, 0); // 5 minutes.

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
			get { return LoggerProvider.GetLogger(typeof(ServiceProcessHelper)); }
		}

		#endregion

		/// <summary>
		/// Determines whether a service is installed.
		/// </summary>
		/// <param name="name">The service name.</param>
		/// <returns><c>true</c> if the service is installed; otherwise, <c>false</c>.</returns>
		public static bool IsServiceInstalled(string name)
		{
			try
			{
				using (ServiceController serviceController = new ServiceController(name))
				{
					ServiceControllerStatus status = serviceController.Status;
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Starts a service.
		/// </summary>
		/// <param name="name">The service name.</param>
		/// <returns><c>true</c> if the service starts; otherwise, <c>false</c>.</returns>
		public static bool StartService(string name)
		{
			if (!IsServiceInstalled(name))
				return false;

			int maxAttempts = 2;

			for (int i = 1; i <= maxAttempts; i++)
			{
				try
				{
					using (ServiceController serviceController = new ServiceController(name))
					{
						if (serviceController.Status != ServiceControllerStatus.Stopped)
							return true;

						serviceController.Start();

						serviceController.WaitForStatus(ServiceControllerStatus.Running, WAIT_TIMEOUT);
						return serviceController.Status == ServiceControllerStatus.Running;
					}
				}
				catch (Exception ex)
				{
					if (i == maxAttempts)
						Logger.Error(ex, "Unable to start service");
				}
			}

			return false;
		}

		/// <summary>
		/// Stops a service.
		/// </summary>
		/// <param name="name">The service name.</param>
		/// <returns><c>true</c> if the service stops; otherwise, <c>false</c>.</returns>
		public static bool StopService(string name)
		{
			if (!IsServiceInstalled(name))
				return false;

			try
			{
				using (ServiceController serviceController = new ServiceController(name))
				{
					if (serviceController.Status != ServiceControllerStatus.Running)
						return true;

					serviceController.Stop();

					serviceController.WaitForStatus(ServiceControllerStatus.Stopped, WAIT_TIMEOUT);
					return serviceController.Status == ServiceControllerStatus.Stopped;
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Unable to stop service");
				return false;
			}
		}

		/// <summary>
		/// Installs a service.
		/// </summary>
		/// <param name="assembly">The assembly to install. as a service</param>
		/// <param name="args">The command line to use when creating a new System.Configuration.Install.InstallContext object for the assembly's installation.</param>
		/// <returns><c>true</c> if the service was installed; otherwise, <c>false</c>.</returns>
		public static bool InstallService(Assembly assembly, string[] args)
		{
			using (AssemblyInstaller installer = new AssemblyInstaller(assembly, args))
			{
				IDictionary state = new Hashtable();

				installer.UseNewContext = true;

				try
				{
					installer.Install(state);
					installer.Commit(state);
				}
				catch (Exception exception)
				{
					Logger.Error(exception, "Unable to install service");

					try
					{
						installer.Rollback(state);
					}
					catch (Exception ex)
					{
						Logger.Error(ex, "Unable to rollback installation service");
					}

					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Uninstalls a service.
		/// </summary>
		/// <param name="assembly">The assembly to install. as a service</param>
		/// <param name="args">The command line to use when creating a new System.Configuration.Install.InstallContext object for the assembly's installation.</param>
		/// <returns><c>true</c> if the service was uninstalled; otherwise, <c>false</c>.</returns>
		public static bool UninstallService(Assembly assembly, string[] args)
		{
			using (AssemblyInstaller installer = new AssemblyInstaller(assembly, args))
			{
				IDictionary state = new Hashtable();

				installer.UseNewContext = true;

				try
				{
					installer.Uninstall(state);
				}
				catch
				{
					try
					{
						installer.Rollback(state);
					}
					catch (Exception ex)
					{
						Logger.Error(ex, "Unable to rollback uninstallation.");
					}

					return false;
				}
			}

			return true;
		}
	}
}
