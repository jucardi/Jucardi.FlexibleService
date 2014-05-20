// Filename: FlexibleServiceConfiguration.cs
// Author:   Jucardi
// Date:     3/26/2014 02:17:08 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        public const string CONFIGURATION_PATH = @".\Configuration";

        private static readonly XmlSerializer EXTENSION_INFO_SER = new XmlSerializer(typeof(ExtensionInfoCollection));

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
            get { return LoggerProvider.GetLogger(typeof(XmlSerializable)); }
        }

        #endregion

        #region Fields

        private static FlexibleServiceConfiguration _configuration = new FlexibleServiceConfiguration();

        private bool                    monitorAssemblies = false;
        private List<NameValue>         commonProperties  = null;
        private ExtensionInfoCollection extensions        = null;
        private XmlElement              loggerXml         = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the _configurationConfiguration.
        /// </summary>
        /// <value>
        /// The Configuration.
        /// </value>
        public static FlexibleServiceConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [monitor assemblies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [monitor assemblies]; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("monitor-assemblies")]
        public bool MonitorAssemblies
        {
            get { return this.monitorAssemblies; }
            set { this.monitorAssemblies = value; }
        }

        /// <summary>
        /// Gets or sets the common properties.
        /// </summary>
        /// <value>
        /// The common properties.
        /// </value>
        [XmlArray("common-values")]
        [XmlArrayItem("property")]
        public List<NameValue> CommonProperties
        {
            get { return this.commonProperties; }
            set { this.commonProperties = value; }
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
        public XmlElement LoggerXml
        {
            get
            {
                return this.loggerXml;
            }

            set
            {
                this.loggerXml = value;
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
            LoadConfigurationDirectory();
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
        /// Loads the configuration files inside a folder.
        /// </summary>
        private static void LoadConfigurationDirectory()
        {
            if (!Directory.Exists(CONFIGURATION_PATH))
                return;

            if (Configuration.Types == null)
                Configuration.Types = new ExtensionInfoCollection();

            string[] files = Directory.GetFiles(CONFIGURATION_PATH, "*.xml");

            foreach (string file in files)
            {
                try
                {
                    using (FileStream fs = File.OpenRead(file))
                    using (XmlReader reader = XmlReader.Create(fs))
                    {
                        if (!EXTENSION_INFO_SER.CanDeserialize(reader))
                            continue;

                        ExtensionInfoCollection config = (ExtensionInfoCollection)EXTENSION_INFO_SER.Deserialize(reader);
                        Configuration.Types.AddRange(config);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error loading configuration file '{0}'", Path.GetFileName(file));
                }
            }
        }

        /// <summary>
        /// Updates the LoggerXml manager settings.
        /// </summary>
        private void UpdateLoggerSettings()
        {
            if (this.LoggerXml != null)
                LoggerProvider.Configure(this.LoggerXml);
            else
                LoggerProvider.Configure();
        }

        #endregion
    }
}
