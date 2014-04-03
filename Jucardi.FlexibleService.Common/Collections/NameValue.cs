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
	/// Name-Value pair container.
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

		private string _name  = string.Empty;
		private object _value = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue"/> class.
		/// </summary>
		public NameValue()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue" /> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public NameValue(string name, object value)
		{
			_name  = name;
			_value = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="NameValue" /> class.
		/// </summary>
		/// <param name="info">The information.</param>
		/// <param name="context">The context.</param>
		/// <exception cref="System.ArgumentNullException">info</exception>
		protected NameValue(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			_name = info.GetString(NAME);
			string valueString = info.GetString(VALUE);

			if (valueString == null)
			{
				_value = null;
				return;
			}

			string klass     = info.GetString("CLASS");
			Type   klassType = string.IsNullOrEmpty(klass) ? Type.GetType(klass) : null;

			if (klassType == typeof(string) || klassType == null)
			{
				_value = valueString;
			}
			else
			{
				TypeConverter converter = TypeDescriptor.GetConverter(klassType);
				_value = converter.ConvertFromInvariantString(valueString);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the parameter Name.
		/// </summary>
		/// <_value>The parameter Name.</_value>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets or sets the parameter Value
		/// </summary>
		/// <_value>The parameter Value.</_value>
		public object Value
		{
			get { return _value;  }
			set { _value = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Assigns the value to the given object if a property with the same name and type is found.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <exception cref="System.ArgumentNullException">object</exception>
		/// <exception cref="System.InvalidOperationException">
		/// </exception>
		public void AssignTo(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			Type objType = obj.GetType();
			PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(objType);

			AssignTo(obj, descriptors);
		}

		/// <summary>
		/// Assigns the value to the given object if a property with the same name and type is found.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="descriptors">The descriptors.</param>
		/// <exception cref="System.InvalidOperationException">
		/// </exception>
		internal void AssignTo(object obj, PropertyDescriptorCollection descriptors)
		{
			PropertyDescriptor descriptor = descriptors[this.Name];
			object value = null;

			if (descriptor == null)
				throw new InvalidOperationException(string.Format("The property '{0}' could not be found.", this.Name));

			if (descriptor.IsReadOnly)
				throw new InvalidOperationException(string.Format("The property '{0}' is read only", this.Name));

			PropertyInfo pinfo = obj.GetType().GetProperty(this.Name);

			if (pinfo == null)
				throw new InvalidOperationException(string.Format("The property '{0}' could not be found.", this.Name));

			if (!pinfo.CanWrite)
				throw new InvalidOperationException(string.Format("The property '{0}' is read only", this.Name));

			MethodInfo setMethod = pinfo.GetSetMethod();

			if (setMethod == null || !setMethod.IsPublic)
				throw new InvalidOperationException(string.Format("The property '{0}' is read only", this.Name));

			object[] attributes = pinfo.GetCustomAttributes(typeof (PropertiesCollectionIgnoreAttribute), true);

			if (attributes.Length > 0)
				throw new InvalidOperationException(string.Format("The property '{0}' cannot be set via PropertyCollection.",
					this.Name));

			value = this.ConvertValue(descriptor);

			if (value != null)
				descriptor.SetValue(obj, value);
		}

		/// <summary>
		/// Creates the parameter Value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>
		/// The parameter Value.
		/// </returns>
		public T ConvertValue<T>()
		{
			object result = ConvertValue(typeof(T));
			return result == null ? default(T) : (T)result;
		}

		/// <summary>
		/// Creates the parameter _value.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// The parameter Value.
		/// </returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public object ConvertValue(Type type)
		{
			if (_value == null)
				return null;

			Type valueType = _value.GetType();

			if (type.IsAssignableFrom(valueType))
				return _value;

			TypeConverter converter = TypeDescriptor.GetConverter(type);

			if (!converter.CanConvertFrom(valueType))
				throw new InvalidOperationException();

			return converter.ConvertFrom(_value);
		}

		/// <summary>
		/// Creates the parameter Value.
		/// </summary>
		/// <param name="descriptor">The descriptor.</param>
		/// <returns>
		/// The parameter Value.
		/// </returns>
		/// <exception cref="System.InvalidOperationException"></exception>
		public object ConvertValue(PropertyDescriptor descriptor)
		{
			if (_value == null)
				return null;

			Type valueType = _value.GetType();

			if (descriptor.PropertyType.IsAssignableFrom(valueType))
				return _value;

			TypeConverter converter = descriptor.Converter;

			if (!converter.CanConvertFrom(valueType))
				throw new InvalidOperationException(string.Format("Unable to convert. {0} (type {1}) to type {2}", descriptor.Name, descriptor.PropertyType.Name, valueType.Name));

			return converter.ConvertFrom(_value);
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
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.</param>
		/// <exception cref="System.NullReferenceException">_name</exception>
		public void ReadXml(XmlReader reader)
		{
			_name = reader.GetAttribute(NAME);

			if (this.Name == null)
				throw new NullReferenceException("Name");

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
					_value = serializer.Deserialize(reader);
					reader.Read();
					return;
				}

				valueString = reader.ReadInnerXml();
			}
			else
			{
				valueString = reader.GetAttribute(VALUE);
			}

			if (valueString != null)
			{
				if (klassType == typeof(string) || klassType == null)
				{
					_value = valueString;
				}
				else
				{

					TypeConverter converter = TypeDescriptor.GetConverter(klassType);
					_value = converter.ConvertFromInvariantString(valueString);
				}

				reader.Read();
			}
			else
			{
				_value = null;
			}
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param _name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
		public void WriteXml(XmlWriter writer)
		{
			if (_value == null)
				return;

			Type          valueType  = _value.GetType();
			Type          stringType = typeof(string);
			TypeConverter converter  = TypeDescriptor.GetConverter(valueType);

			writer.WriteAttributeString(NAME, _name);

			if (_value is string || !valueType.IsClass)
			{
				writer.WriteAttributeString(VALUE, _value.ToString());
			}
			else if (converter.CanConvertTo(stringType) && converter.CanConvertFrom(stringType))
			{
				writer.WriteAttributeString(VALUE, converter.ConvertToInvariantString(_value));
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
					valueSerializer.Serialize(writer, _value);
				}
				catch (Exception ex)
				{
					throw new SerializationException("Unable to deserialize Value", ex);
				}

				return;
			}

			writer.WriteAttributeString(CLASS, valueType.FullName);
		}

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.</param>
		/// <exception cref="System.ArgumentNullException">info</exception>
		/// <exception cref="System.Runtime.Serialization.SerializationException"></exception>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission.</exception>
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			if (_value == null)
				return;

			Type valueType  = _value.GetType();
			Type stringType = typeof(string);
			TypeConverter converter = TypeDescriptor.GetConverter(valueType);

			info.AddValue(NAME, _name);

			if (_value is string || !valueType.IsClass)
			{
				info.AddValue(VALUE, _value.ToString());
			}
			else if (converter.CanConvertTo(stringType) && converter.CanConvertFrom(stringType))
			{
				info.AddValue(VALUE, converter.ConvertToInvariantString(_value));
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
			return new NameValue(_name == null ? null : (string)_name.Clone(), _value);
		}

		#endregion
	}
}
