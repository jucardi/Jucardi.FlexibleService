// Filename: XmlSerializable.cs
// Author:   Jucardi
// Date:     3/26/2014 02:16:04 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Jucardi.FlexibleService.Common.Serialization
{
	public class XmlSerializable
	{
		#region Fields

		private XmlSerializer serializer;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlSerializable"/> class.
		/// </summary>
		protected XmlSerializable()
		{
			this.serializer = new XmlSerializer(this.GetType());
		}

		#endregion

		#region Methods

		/// <summary>
		/// Saves the XML serialization of this instance into the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		public virtual void Save(string path)
		{
			FileStream stream = new FileStream(path, FileMode.Create);
			Save(stream);
			stream.Close();
		}

		/// <summary>
		/// Saves the XML serialization of this instance into the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public virtual void Save(Stream stream)
		{
			this.serializer.Serialize(stream, this);
		}

		/// <summary>
		/// Loads the specified XML file into this instance.
		/// </summary>
		/// <param name="path">The path.</param>
		public virtual void Load(string path)
		{
			if (!File.Exists(path))
				return;

			StreamReader streamReader = new StreamReader(path);
			Load(streamReader);
		}

		/// <summary>
		/// Loads the specified XML stream into this instance.
		/// </summary>
		/// <param name="stream">The stream.</param>
		public virtual void Load(Stream stream)
		{
			StreamReader streamReader = new StreamReader(stream);
			Load(streamReader);
		}

		/// <summary>
		/// Loads the specified stream reader.
		/// </summary>
		/// <param name="streamReader">The stream reader.</param>
		public virtual void Load(StreamReader streamReader)
		{
			XmlTextReader xmlTextReader = new XmlTextReader(streamReader);

			this.Load(xmlTextReader);

			streamReader.Close();
		}

		/// <summary>
		/// Loads the specified element.
		/// </summary>
		/// <param name="element">The element.</param>
		public virtual void Load(XmlElement element)
		{
			StringBuilder xmlTextBuilder = new StringBuilder();
			XmlWriter     xmlWriter      = new XmlTextWriter(new StringWriter(xmlTextBuilder));

			element.WriteTo(xmlWriter);

			StringReader stringReader = new StringReader(xmlTextBuilder.ToString());
			XmlReader    reader       = XmlReader.Create(stringReader);

			this.Load(reader);
		}

		/// <summary>
		/// Loads the specified reader.
		/// </summary>
		/// <param name="reader">The reader.</param>
		public virtual void Load(XmlReader reader)
		{
			if (this.serializer.CanDeserialize(reader))
			{
				object         c          = this.serializer.Deserialize(reader);
				Type           t          = this.GetType();
				PropertyInfo[] properties = t.GetProperties();

				foreach (PropertyInfo p in properties)
				{
					if (!p.CanWrite || p.GetCustomAttributes(typeof(XmlIgnoreAttribute), true).Length > 0)
						continue;

					p.SetValue(this, p.GetValue(c, null), null);
				}

				IDisposable dispObj = c as IDisposable;

				if (dispObj != null)
					dispObj.Dispose();
			}

			reader.Close();
		}

		/// <summary>
		/// Loads from text.
		/// </summary>
		/// <param name="xmlText">The XML text.</param>
		public virtual void LoadFromText(string xmlText)
		{
			byte[] byteArray = Encoding.UTF8.GetBytes(xmlText);

			using (MemoryStream ms = new MemoryStream(byteArray))
				this.Load(ms);
		}

		#endregion
	}
}
