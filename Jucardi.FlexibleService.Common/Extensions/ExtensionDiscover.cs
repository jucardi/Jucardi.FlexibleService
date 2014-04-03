// Filename: ExtensionDiscover.cs
// Author:   Jucardi
// Date:     3/26/2014 11:59:04 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService.Common.Extensions
{
	/// <summary>
	/// Discover assembly files extensions.
	/// <remarks>This class is helpful for discover extensions without loading the assembly in the current domain.</remarks>
	/// </summary>
	/// <typeparam name="T">The class type to discover in the assemblies.</typeparam>
	public class ExtensionDiscover<T> : MarshalByRefObject
	{
		#region Fields

		private string assemblyDirectory = string.Empty;

		#endregion

		#region Events

		/// <summary>
		/// Occurs when [file loaded].
		/// </summary>
		public event Action FileLoaded;

		#endregion

		#region Logger

		/// <summary>
		/// Gets the logger.
		/// </summary>
		/// <value>
		/// The logger.
		/// </value>
		protected static ILoggerEx Logger
		{
			get { return LoggerProvider.GetLogger(typeof(ExtensionDiscover<T>)); }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Discovers extensions in the specified path.
		/// </summary>
		/// <param name="assemblyPath">The assembly path.</param>
		/// <param name="token">The token.</param>
		/// <returns>
		/// The list of assemblies that contains the class type to discover.
		/// </returns>
		public ArrayList Discover(string assemblyPath, string token)
		{
			assemblyPath = Path.GetFullPath(assemblyPath);

			if (!File.Exists(assemblyPath))
				return new ArrayList();

			Type      extensionType = typeof(T);
			ArrayList result        = new ArrayList();
			Type[]    types         = null;

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.OnAssemblyResolve);

			this.assemblyDirectory = Path.GetDirectoryName(assemblyPath);

			try
			{
				Assembly assembly = Assembly.LoadFile(assemblyPath);

				if (assembly == null)
				{
					Logger.Warn("Assembly not found '{0}'.", Path.GetFileName(assemblyPath));
					return new ArrayList();
				}

				// Check token if it is not null.
				if (!string.IsNullOrEmpty(token) &&
				    String.Compare(Convert.ToBase64String(assembly.GetName().GetPublicKeyToken()), token, StringComparison.Ordinal) != 0)
				{
					Logger.Warn("Incorrect token in assembly '{0}'", Path.GetFileName(assemblyPath));
					return new ArrayList();
				}

				types = assembly.GetTypes();
			}
			catch (Exception ex)
			{
				Logger.Error(ex, "Error occured trying to load assembly '{0}'", assemblyPath);
				return new ArrayList();
			}
			finally
			{
				if (this.FileLoaded != null)
					this.FileLoaded();
			}

			foreach (Type type in types)
			{
				if (!type.IsClass || type.IsAbstract || !extensionType.IsAssignableFrom(type))
					continue;

				result.Add(type.FullName);
				break;
			}

			AppDomain.CurrentDomain.AssemblyResolve -= this.OnAssemblyResolve;
			return result;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Called when assembly resolve is required.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.ResolveEventArgs"/> instance containing the event data.</param>
		/// <returns>The assembly object.</returns>
		protected Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			string name = string.Format("{0}.dll", args.Name.Split(',')[0]);
			string path = Path.GetFullPath(Path.Combine(this.assemblyDirectory, name));

			if (File.Exists(path))
				return Assembly.LoadFile(path);

			return null;
		}

		#endregion
	}
}
