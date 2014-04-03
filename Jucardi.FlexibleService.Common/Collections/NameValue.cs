// Filename: NameValue.cs
// Author:   Jucardi
// Date:     3/23/2014 06:02:37 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Jucardi.FlexibleService.Common.Collections
{
	/// <summary>
	/// Name-value pair container.
	/// </summary>
	[Serializable]
	public class NameValue : ISerializable, IXmlSerializable, ICloneable
	{
		#region Constants

		internal const string CLASS = "class";
		internal const string NAME  = "name";
		internal const string VALUE = "value";

		#endregion

		#region Fields

		private string name  = string.Empty;
		private object value = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue"/> class.
		/// </summary>
		public NameValue()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue"/> class.
		/// </summary>
		/// <param name="name">The parameter name.</param>
		/// <param name="value">The parameter value.</param>
		public NameValue(string name, object value)
		{
			this.name  = name;
			this.value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue"/> class.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The streaming context.</param>
		protected NameValue(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			string valueString = string.Empty;

			this.name   = info.GetString(NAME);
			valueString = info.GetString(VALUE);

			if (valueString == null)
			{
				this.value = null;
				return;
			}

			string klass     = info.GetString("CLASS");
			Type   klassType = string.IsNullOrEmpty(klass) ? Type.GetType(klass) : null;

			if (klassType == typeof(string) || klassType == null)
			{
				this.value = valueString;
			}
			else
			{
				TypeConverter converter = TypeDescriptor.GetConverter(klassType);
				this.value = converter.ConvertFromInvariantString(valueString);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the parameter name.
		/// </summary>
		/// <value>The parameter name.</value>
		public string Name
		{
			get { return this.name; }
			set { this.name = value; }
		}

		/// <summary>
		/// Gets or sets the parameter value.
		/// </summary>
		/// <value>The parameter value.</value>
		public object Value
		{
			get { return this.value;  }
			set { this.value = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Creates the parameter value.
		/// </summary>
		/// <typeparam name="T">The expected type of the parameter value object.</typeparam>
		/// <returns>The parameter value.</returns>
		public T ConvertValue<T>()
		{
			object result = ConvertValue(typeof(T));
			return result == null ? default(T) : (T)result;
		}

		/// <summary>
		/// Creates the parameter value.
		/// </summary>
		/// <param name="type">The expected type of the parameter value object.</param>
		/// <returns>The parameter value.</returns>
		public object ConvertValue(Type type)
		{
			if (this.value == null)
				return null;

			Type valueType = this.value.GetType();

			if (type.IsAssignableFrom(valueType))
				return this.value;

			TypeConverter converter = TypeDescriptor.GetConverter(type);

			if (!converter.CanConvertFrom(valueType))
				throw new InvalidOperationException();

			return converter.ConvertFrom(this.value);
		}

		/// <summary>
		/// Creates the parameter value.
		/// </summary>
		/// <param name="descriptor">The property descriptor.</param>
		/// <returns>The parameter value.</returns>
		public object ConvertValue(PropertyDescriptor descriptor)
		{
			if (this.value == null)
				return null;

			Type valueType = this.value.GetType();

			if (descriptor.PropertyType.IsAssignableFrom(valueType))
				return this.value;

			TypeConverter converter = descriptor.Converter;

			if (!converter.CanConvertFrom(valueType))
				throw new InvalidOperationException(string.Format("Unable to convert. {0} (type {1}) to type {2}", descriptor.Name, descriptor.PropertyType.Name, valueType.Name));

			return converter.ConvertFrom(this.value);
		}

		/// <summary>
		/// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method,
		/// and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the
		/// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the
		/// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.
		/// </returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
		public void ReadXml(XmlReader reader)
		{
			this.name = reader.GetAttribute(NAME);

			if (this.name == null)
				throw new NullReferenceException("name");

			string klass       = reader.GetAttribute(CLASS);
			string valueString = null;
			Type   klassType   = !string.IsNullOrEmpty(klass) ? Type.GetType(klass) : null;

			if (!string.IsNullOrEmpty(klass) && klassType == null)
			{
				foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
				{
					klassType = assem.GetType(klass);

					if (klassType != null)
						break;
				}
			}

			if (!reader.IsEmptyElement)
			{
				reader.Read();

				XmlSerializer serializer = null;

				try
				{
					XmlAttributes         xmlAttributes = new XmlAttributes();
					XmlRootAttribute      rootAttribute = new XmlRootAttribute(VALUE);
					XmlAttributeOverrides xmlOverride   = new XmlAttributeOverrides();

					xmlAttributes.XmlRoot = rootAttribute;
					xmlOverride.Add(klassType, xmlAttributes);

					serializer = new XmlSerializer(klassType, xmlOverride);
				}
				catch
				{
				}

				if (serializer != null && serializer.CanDeserialize(reader))
				{
					this.value = serializer.Deserialize(reader);
					reader.Read();
					return;
				}
				else
				{
					valueString = reader.ReadInnerXml();
				}
			}
			else
			{
				valueString = reader.GetAttribute(VALUE);
			}

			if (valueString != null)
			{
				if (klassType == typeof(string) || klassType == null)
				{
					this.value = valueString;
				}
				else
				{

					TypeConverter converter = TypeDescriptor.GetConverter(klassType);
					this.value = converter.ConvertFromInvariantString(valueString);
				}

				reader.Read();
			}
			else
			{
				this.value = null;
			}
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
		public void WriteXml(XmlWriter writer)
		{
			if (this.value == null)
				return;

			Type          valueType  = this.value.GetType();
			Type          stringType = typeof(string);
			TypeConverter converter  = TypeDescriptor.GetConverter(valueType);

			writer.WriteAttributeString(NAME, this.name);

			if (this.value is string || !valueType.IsClass)
			{
				writer.WriteAttributeString(VALUE, this.value.ToString());
			}
			else if (converter.CanConvertTo(stringType) && converter.CanConvertFrom(stringType))
			{
				writer.WriteAttributeString(VALUE, converter.ConvertToInvariantString(this.value));
			}
			else
			{
				writer.WriteAttributeString(CLASS, valueType.FullName);

				try
				{
					XmlAttributes         xmlAttributes = new XmlAttributes();
					XmlRootAttribute      rootAttribute = new XmlRootAttribute(VALUE);
					XmlAttributeOverrides xmlOverride   = new XmlAttributeOverrides();

					xmlAttributes.XmlRoot = rootAttribute;
					xmlOverride.Add(valueType, xmlAttributes);

					XmlSerializer valueSerializer = new XmlSerializer(valueType, xmlOverride);
					valueSerializer.Serialize(writer, this.value);
				}
				catch (Exception ex)
				{
					throw new SerializationException("Unable to deserialize value", ex);
				}

				return;
			}

			writer.WriteAttributeString(CLASS, valueType.FullName);
		}

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (this.value == null)
				return;

			Type valueType  = this.value.GetType();
			Type stringType = typeof(string);
			TypeConverter converter = TypeDescriptor.GetConverter(valueType);

			info.AddValue(NAME, this.name);

			if (this.value is string || !valueType.IsClass)
			{
				info.AddValue(VALUE, this.value.ToString());
			}
			else if (converter.CanConvertTo(stringType) && converter.CanConvertFrom(stringType))
			{
				info.AddValue(VALUE, converter.ConvertToInvariantString(this.value));
				info.AddValue(CLASS, valueType.FullName);
			}
			else
			{
				throw new SerializationException();
			}
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public object Clone()
		{
			return new NameValue(this.name == null ? null : (string)this.name.Clone(), this.value);
		}

		#endregion
	}
}
