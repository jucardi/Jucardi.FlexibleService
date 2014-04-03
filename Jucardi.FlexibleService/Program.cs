// Filename: Program.cs
// Author:   Jucardi
// Date:     3/23/2014 11:10:16 AM
//
// The use of this code is subject to Open Source License Agreement.

using Jucardi.FlexibleService.Common.Extensions;
using Jucardi.FlexibleService.Common.Log;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Jucardi.FlexibleService.Common.Service
{
	internal class Program : ServiceBase
	{
		#region Fields

		internal const string SERVICE_NAME     = "Jucardi Flexible Service";
		internal const string SERVICE_NAME_KEY = "jucardiFlex";

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
			get { return LoggerProvider.GetLogger(typeof (Program)); }
		}

		#endregion

		#region Protected Methods

		/// <summary>
		/// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control
		/// Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to
		/// take when the service starts.
		/// </summary>
		/// <param name="args">Data passed by the start command.</param>
		protected override void OnStart(string[] args)
		{
			WorkerFactory.Start();
			base.OnStart(args);
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
		/// </summary>
		protected override void OnPause()
		{
			base.OnPause();
		}

		/// <summary>
		/// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/// <param name="args">The program arguments.</param>
		/// <returns>The program error code.</returns>
		[STAThread]
		internal static int Main(string[] args)
		{
			FlexibleServiceConfiguration.Initialize();

			bool runAsConsole = false;
			bool startService = false;
			bool stopService  = false;
			bool install      = false;
			bool uninstall    = false;

			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);

			Directory.SetCurrentDirectory(Path.GetDirectoryName(typeof(Program).Assembly.Location));

			// Parse program arguments.
			foreach (string arg in args)
			{
				switch (arg)
				{
					case "/install":
						install = true;
						break;

					case "/uninstall":
						uninstall = true;
						break;

					case "/start":
						startService = true;
						break;

					case "/stop":
						stopService = true;
						break;

					case "/console":
						runAsConsole = true;
						break;

					case "/help":
					case "/h":
					case "/?":
						PrintHelp();
						return 0;

					default:
						Console.WriteLine("Invalid command.");
						PrintHelp();
						return -1;
				}
			}

			if (args.Length > 1)
			{
				Console.WriteLine("Only one option is allowed.");
				return 1;
			}

			if (install)
				return InstallService(args);

			if (uninstall)
				return UninstallService(args);

			if (startService)
				return StartService();

			if (stopService)
				return StopService();

			if (runAsConsole)
				return RunAsConsole();

			return RunAsWindowsService();
		}

		/// <summary>
		/// Runs the service as a console application.
		/// </summary>
		/// <returns>Return 0 if the operation was successful; otherwise, <c>-1</c>.</returns>
		private static int RunAsConsole()
		{
			WorkerFactory.Start();
			Console.WriteLine("Press any key to exit.");
			Console.ReadKey();
			return 0;
		}

		/// <summary>
		/// Runs the service as a windows service.
		/// </summary>
		/// <returns>Return 0 if the operation was successful; otherwise, <c>-1</c>.</returns>
		private static int RunAsWindowsService()
		{
			try
			{
				ServiceBase[] services = new ServiceBase[] { new Program() };
				ServiceBase.Run(services);
			}
			catch
			{
				Console.WriteLine("Unable to start as service.");
				return -1;
			}

			return 0;
		}

		/// <summary>
		/// Starts the service as a service.
		/// </summary>
		/// <returns>Return 0 if the operation was successful; otherwise, <c>-1</c>.</returns>
		private static int StartService()
		{
			if (!ServiceProcessHelper.StartService(SERVICE_NAME))
			{
				Console.WriteLine("Unable to start service.");
				return -1;
			}

			Console.WriteLine("Service started.");
			return 0;
		}

		/// <summary>
		/// Stops the service as a service.
		/// </summary>
		/// <returns>Return 0 if the operation was successful; otherwise, <c>-1</c>.</returns>
		private static int StopService()
		{
			if (!ServiceProcessHelper.StopService(SERVICE_NAME))
			{
				Console.WriteLine("Unable to stop service.");
				return -1;
			}

			Console.WriteLine("Service stopped.");
			return 0;
		}

		/// <summary>
		/// Installs the windows service.
		/// </summary>
		/// <param name="args">The program arguments.</param>
		/// <returns>Return <c>0</c> if the operation was successful; otherwise, <c>-1</c>.</returns>
		private static int InstallService(string[] args)
		{
			if (!ServiceProcessHelper.InstallService(typeof(Program).Assembly, args))
			{
				Console.WriteLine("Unable to install service.");
				return -1;
			}

			Console.WriteLine("Service installed.");
			return 0;
		}

		/// <summary>
		/// Uninstalls the windows service.
		/// </summary>
		/// <param name="args">The program arguments.</param>
		/// <returns>return 0 if uninstall service is accomplished or -1 when an error occurs.</returns>
		private static int UninstallService(string[] args)
		{
			if (!ServiceProcessHelper.UninstallService(typeof(Program).Assembly, args))
			{
				Console.WriteLine("Unable to uninstall service.");
				return -1;
			}

			Console.WriteLine("Service uninstalled.");
			return 0;
		}

		/// <summary>
		/// Prints the help information.
		/// </summary>
		private static void PrintHelp()
		{
			Console.WriteLine(SERVICE_NAME);
			Console.WriteLine(string.Empty);
			Console.WriteLine("{0} [/install | /uninstall | /start | /stop | /console | /help] [/config config_file]", Process.GetCurrentProcess().ProcessName);
			Console.WriteLine(string.Empty);
			Console.WriteLine("/install		{0}", "Installs the service.");
			Console.WriteLine("/uninstall		{0}", "Uninstalls the service.");
			Console.WriteLine("/start			{0}", "Starts the service.");
			Console.WriteLine("/stop			{0}", "Stops the service.");
			Console.WriteLine("/console		{0}", "Launches the sevice as a console application.");
			Console.WriteLine("/help /h /?		{0}", "Prints the help in the console.");
			Console.WriteLine(string.Empty);
		}

		/// <summary>
		/// Called when a unhanded exception occurred.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
		private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Logger.Error(e.ExceptionObject.ToString(), "Unhandled exception occured");
		}

		#endregion
	}
}
