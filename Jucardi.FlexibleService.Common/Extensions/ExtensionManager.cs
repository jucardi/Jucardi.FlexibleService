// Filename: ExtensionManager.cs
// Author:   Jucardi
// Date:     3/24/2014 10:17:11 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace Jucardi.FlexibleService.Common.Extensions
{
	/// <summary>
	/// Discover assembly files extensions.
	/// <remarks>This class is helpful for dis
	/// cover extensions without loading the assembly in the current domain.</remarks>
	/// </summary>
	public static class ExtensionManager
	{
		#region Methods

		/// <summary>
		/// Gets a list of assemblies which contain at least one class which implements the given type.
		/// </summary>
		/// <typeparam name="T1">The search type.</typeparam>
		/// <param name="path">The path.</param>
		/// <returns>The list of assemblies.</returns>
		public static Dictionary<string, string> GetWorkersCollection<T1>(string path)
		{
			Type           remoteType = typeof(ExtensionDiscover<T1>);
			AppDomainSetup domainInfo = new AppDomainSetup();

			// Prepares a different domain for extension assemblies search. This isolates the loaded assemblies from the application domain.
			domainInfo.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

			AppDomain                  domain   = AppDomain.CreateDomain(Guid.NewGuid().ToString().GetHashCode().ToString("x"), null, domainInfo);
			ExtensionDiscover<T1>      discover = (ExtensionDiscover<T1>)domain.CreateInstanceAndUnwrap(remoteType.Assembly.FullName, remoteType.FullName);
			Dictionary<string, string> ret      = new Dictionary<string, string>();
			string                     token    = Convert.ToBase64String(Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken());
			string[]                   files    = Directory.GetFiles(path, "*.dll");

			foreach (string file in files)
			{
				ArrayList types = discover.Discover(path, token);

				if (types.Count == 0)
					continue;

				foreach (string type in types)
					ret.Add(type, file);
			}

			return ret;
		}

		/// <summary>
		/// Loads the assemblies.
		/// </summary>
		/// <typeparam name="T1">The search type.</typeparam>
		/// <param name="path">The path.</param>
		/// <returns>The types collection found.</returns>
		public static Dictionary<string, Type> LoadAssemblies<T1>(string path)
		{
			Type           remoteType = typeof(ExtensionDiscover<T1>);
			AppDomainSetup domainInfo = new AppDomainSetup();

			// Prepares a different domain for extension assemblies search. This isolates the loaded assemblies from the application domain.
			domainInfo.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;

			AppDomain             domain   = AppDomain.CreateDomain(Guid.NewGuid().ToString().GetHashCode().ToString("x"), null, domainInfo);
			ExtensionDiscover<T1> discover = (ExtensionDiscover<T1>)domain.CreateInstanceAndUnwrap(remoteType.Assembly.FullName, remoteType.FullName);

			string    token      = Convert.ToBase64String(Assembly.GetExecutingAssembly().GetName().GetPublicKeyToken());
			ArrayList files      = discover.Discover(path, token);
			Type      searchType = typeof(T1);

			Dictionary<string, Type> ret = new Dictionary<string, Type>();

			// Search for valid extension assemblies.
			foreach (string file in files)
			{
				Assembly assembly = Assembly.LoadFile(file);
				Type[]   types    = assembly.GetTypes();

				foreach (Type type in types)
				{
					if (!type.IsClass || type.IsAbstract || !searchType.IsAssignableFrom(type))
						continue;

					string addInName = GetExtensionName(type);

					addInName = string.IsNullOrEmpty(addInName) ? type.Name : addInName;

					ret[addInName] = type;
				}
			}

			return ret;
		}

		/// <summary>
		/// Gets the name of the extension.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The extension name.</returns>
		public static string GetExtensionName(Type type)
		{
			object[] attributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), true);

			if (attributes.Length == 0)
				return string.Empty;

			DisplayNameAttribute attribute = (DisplayNameAttribute)attributes[attributes.Length - 1];

			return attribute.DisplayName;
		}

		#endregion
	}
}

