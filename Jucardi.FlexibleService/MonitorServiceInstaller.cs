// Filename: ServiceProcessHelper.cs
// Author:   Jucardi
// Date:     3/23/2014 11:36:15 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.ServiceProcess;
using System.ComponentModel;
using System.Configuration.Install;

namespace Jucardi.FlexibleService.Common.Service
{
	/// <summary>
	/// Monitor service installer
	/// </summary>
	[RunInstaller(true)]
	public class MonitorServiceInstaller : Installer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MonitorServiceInstaller" /> class.
		/// </summary>
		public MonitorServiceInstaller()
		{
			ServiceProcessInstaller processInstaller = new ServiceProcessInstaller();
			ServiceInstaller        serviceInstaller = new ServiceInstaller();

			////set the privileges
			processInstaller.Account  = ServiceAccount.LocalSystem;
			processInstaller.Password = null;
			processInstaller.Username = null;

			serviceInstaller.ServiceName = Program.SERVICE_NAME;
			serviceInstaller.DisplayName = Program.SERVICE_NAME;
			serviceInstaller.Description = Program.SERVICE_NAME;
			serviceInstaller.StartType   = ServiceStartMode.Automatic;

			this.Installers.Add(processInstaller);
			this.Installers.Add(serviceInstaller);
		}
	}
}
