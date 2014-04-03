// Filename: WorkerFactory.cs
// Author:   Jucardi
// Date:     3/24/2014 10:16:16 AM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Jucardi.FlexibleService.Common.Collections;
using Jucardi.FlexibleService.Common.Log;

namespace Jucardi.FlexibleService.Common.Extensions
{
	/// <summary>
	/// Extension assembly information.
	/// </summary>
	[Serializable]
	public class ExtensionInfo : ICloneable
	{
		#region Constants

		private const string NAME       = "name";
		private const string ASSEMBLY   = "assembly";
		private const string TOKEN      = "token";
		private const string CLASS      = "class";
		private const string PROPERTIES = "properties";

		#endregion

		#region Fields

		private string               name                     = string.Empty;
		private string               assembly                 = string.Empty;
		private string               token                    = string.Empty;
		private string               klass                    = string.Empty;
		private PropertiesCollection properties               = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfo"/> class.
		/// </summary>
		public ExtensionInfo()
		{
			this.properties = new PropertiesCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfo"/> class.
		/// </summary>
		/// <param name="name">The friendly name for the extension.</param>
		/// <param name="assemblyPath">The extension assembly path.</param>
		/// <param name="token">The assembly  public key token.</param>
		/// <param name="className">The type name of the extension class.</param>
		/// <param name="properties">The properties collection.</param>
		public ExtensionInfo(string name, string assemblyPath, string token, string className, PropertiesCollection properties)
		{
			this.name       = name;
			this.assembly   = assemblyPath;
			this.token      = token;
			this.klass      = className;
			this.properties = properties ?? new PropertiesCollection();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfo"/> class.
		/// </summary>
		/// <param name="name">The extension friendly name.</param>
		/// <param name="extension">The extension object.</param>
		public ExtensionInfo(string name, object extension)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (extension == null)
				throw new ArgumentNullException("extension");

			Type type     = extension.GetType();
			Uri  path     = new Uri(type.Assembly.Location);
			Uri  relative = new Uri(Assembly.GetExecutingAssembly().Location);

			this.name       = name;
			this.klass      = type.FullName;
			this.assembly   = relative.MakeRelativeUri(path).ToString();
			this.token      = Convert.ToBase64String(type.Assembly.GetName().GetPublicKeyToken());
			this.properties = new PropertiesCollection(extension);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfo"/> class.
		/// </summary>
		/// <param name="name">The extension friendly name.</param>
		/// <param name="type">The extension class type.</param>
		public ExtensionInfo(string name, Type type)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (type == null)
				throw new ArgumentNullException("type");

			Uri path     = new Uri(type.Assembly.Location);
			Uri relative = new Uri(Assembly.GetExecutingAssembly().Location);

			this.name       = name;
			this.klass      = type.FullName;
			this.assembly   = relative.MakeRelativeUri(path).ToString();
			this.token      = Convert.ToBase64String(type.Assembly.GetName().GetPublicKeyToken());
			this.properties = new PropertiesCollection();
		}

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
			get { return LoggerProvider.GetLogger(typeof(ExtensionInfo)); }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the extension friendly name.
		/// </summary>
		/// <value>The extension friendly name.</value>
		[XmlAttribute(NAME)]
		public string Name
		{
			get { return this.name; }
			set { this.name = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the extension assembly path.
		/// </summary>
		/// <value>The extension assembly path.</value>
		[XmlAttribute(ASSEMBLY)]
		public string AssemblyPath
		{
			get { return this.assembly; }
			set { this.assembly = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the assembly public key token.
		/// </summary>
		/// <value>The assembly public key token as a BASE64 encoded string.</value>
		[XmlAttribute(TOKEN)]
		public string Token
		{
			get { return this.token; }
			set { this.token = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the name of the extension class.
		/// </summary>
		/// <value>The name of the extension class.</value>
		[XmlAttribute(CLASS)]
		public string ClassName
		{
			get { return this.klass; }
			set { this.klass = value ?? string.Empty; }
		}

		/// <summary>
		/// Gets or sets the extension class properties.
		/// </summary>
		/// <value>The extension class properties.</value>
		[XmlElement(PROPERTIES)]
		public PropertiesCollection Properties
		{
			get { return this.properties; }
			set { this.properties = value ?? new PropertiesCollection(); }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Configures the specified obj.
		/// </summary>
		/// <param name="obj">The obj.</param>
		/// <returns></returns>
		public bool Configure(object obj)
		{
			if (obj == null)
				return false;

			Type objType = obj.GetType();

			if (this.ClassName != objType.FullName)
				return false;

			if (!string.IsNullOrEmpty(this.Token) && this.Token != Convert.ToBase64String(objType.Assembly.GetName().GetPublicKeyToken()))
				return false;

			PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(objType);

			foreach (NameValue item in this.Properties)
			{
				PropertyDescriptor descriptor = descriptors[item.Name];
				PropertyInfo       pinfo      = objType.GetProperty(item.Name);
				object             value      = null;

				if (descriptor == null || descriptor.IsReadOnly || pinfo == null || !pinfo.CanWrite)
				{
					Logger.Warn("Property '{0}' not set for object of type '{1}'. Property setter does not exist.", item.Name, obj.GetType().ToString());
					continue;
				}

				MethodInfo setMethod = pinfo.GetSetMethod();

				if (setMethod == null || !setMethod.IsPublic)
				{
					Logger.Warn("Property '{0}' not set for object of type '{1}'. Property setter is not public.", item.Name, obj.GetType().ToString());
					continue;
				}

				value = item.ConvertValue(descriptor);

				if (value != null)
					descriptor.SetValue(obj, value);
			}

			return true;
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public object Clone()
		{
			ExtensionInfo result = new ExtensionInfo();

			result.name                     = (string)this.name.Clone();
			result.assembly                 = (string)this.assembly.Clone();
			result.token                    = (string)this.token.Clone();
			result.klass                    = (string)this.klass.Clone();
			result.properties               = (PropertiesCollection)this.properties.Clone();

			return result;
		}

		#endregion
	}
}
