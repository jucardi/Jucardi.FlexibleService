// Filename: FlexibleServiceConfiguration.cs
// Author:   Jucardi
// Date:     3/26/2014 02:17:08 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Xml;
using System.Xml.Serialization;
using Jucardi.FlexibleService.Common.Collections;
using Jucardi.FlexibleService.Common.Log;
using Jucardi.FlexibleService.Common.Serialization;

namespace Jucardi.FlexibleService.Common
{
	[XmlRoot("flexible-service-configuration")]
	[Serializable]
	public class FlexibleServiceConfiguration : XmlSerializable
	{
		#region Constants

		public const string CONFIGURATION_FILE = "Jucardi.FlexibleService.config.xml";

		#endregion

		#region Fields

		private static FlexibleServiceConfiguration _configuration = new FlexibleServiceConfiguration();

		private ExtensionInfoCollection extensions = null;
		private XmlElement logger = null;

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the _configuration.
		/// </summary>
		/// <value>
		/// The _configuration.
		/// </value>
		public static FlexibleServiceConfiguration Configuration
		{
			get { return _configuration; }
			set { _configuration = value; }
		}

		/// <summary>
		/// Gets or sets the extension types _configuration collection.
		/// </summary>
		/// <value>The extension types _configuration collection.</value>
		[XmlElement("types")]
		public ExtensionInfoCollection Types
		{
			get { return this.extensions; }
			set { this.extensions = value ?? new ExtensionInfoCollection(); }
		}

		/// <summary>
		/// Gets or sets the log4net _configuration.
		/// </summary>
		/// <value>The log4net _configuration</value>
		[XmlElement("logger-configuration")]
		public XmlElement Logger
		{
			get
			{
				return this.logger;
			}

			set
			{
				this.logger = value;
				this.UpdateLoggerSettings();
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Initializes this instance.
		/// </summary>
		public static void Initialize()
		{
			_configuration = new FlexibleServiceConfiguration();
			LoadFile(CONFIGURATION_FILE);
		}

		/// <summary>
		/// Loads the specified file.
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		public static void LoadFile(string fileName)
		{
			Configuration.Load(fileName);
		}

		/// <summary>
		/// Updates the logger manager settings.
		/// </summary>
		private void UpdateLoggerSettings()
		{
			if (this.Logger != null)
				LoggerProvider.Configure(this.Logger);
			else
				LoggerProvider.Configure();
		}

		#endregion
	}
}
