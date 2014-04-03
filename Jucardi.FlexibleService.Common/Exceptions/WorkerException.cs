// Filename: WorkerException.cs
// Author:   Jucardi
// Date:     4/2/2014 11:50:43 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;

namespace Jucardi.FlexibleService.Common.Exceptions
{
	internal class WorkerException : ApplicationException
	{
		#region Fields

		private string _workerName;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException" /> class.
		/// </summary>
		/// <param name="workerName">Name of the worker.</param>
		/// <param name="message">A message that describes the error.</param>
		public WorkerException(string workerName, string message)
			: base(message)
		{
			_workerName = workerName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WorkerException" /> class.
		/// </summary>
		/// <param name="workerName">Name of the worker.</param>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
		public WorkerException(string workerName, string message, Exception innerException)
			: base(message, innerException)
		{
			_workerName = workerName;
		}


		#endregion

		#region Properties

		/// <summary>
		/// Gets the name of the worker.
		/// </summary>
		/// <value>
		/// The name of the worker.
		/// </value>
		public string WorkerName
		{
			get { return _workerName; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
		/// </PermissionSet>
		public override string ToString()
		{
			return string.Format("Worker name: {0}\r\n{1}", _workerName, base.ToString());
		}

		#endregion
	}
}
