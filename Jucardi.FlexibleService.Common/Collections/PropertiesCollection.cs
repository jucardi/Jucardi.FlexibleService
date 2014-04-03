// Filename: PropertiesCollection.cs
// Author:   Jucardi
// Date:     3/23/2014 06:19:11 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Jucardi.FlexibleService.Common.Collections
{
	/// <summary>
	/// Properties name-value collection.
	/// </summary>
	[Serializable]
	public class PropertiesCollection : IEnumerable<NameValue>, ISerializable, IXmlSerializable, ICloneable
	{
		#region Constants

		internal const string NAMEVALUE          = "name-value";
		internal const string PROPERTY_TAG       = "property";
		internal const string CLASS_ATTRIBUTE    = "class";

		#endregion

		#region Fields

		private Dictionary<string, NameValue> list = new Dictionary<string, NameValue>();
		private string klass = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesCollection"/> class.
		/// </summary>
		public PropertiesCollection()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesCollection"/> class.
		/// </summary>
		/// <param name="obj">The object to extract the properties information.</param>
		public PropertiesCollection(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("object");

			Type type = obj.GetType();

			PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (PropertyInfo info in properties)
			{
				if (!info.CanWrite || !info.GetSetMethod(true).IsPublic)
					continue;

				object[] attributes = info.GetCustomAttributes(typeof(PropertiesCollectionIgnoreAttribute), true);

				if (attributes != null && attributes.Length > 0)
					continue;

				object value = info.GetValue(obj, null);
				attributes = info.GetCustomAttributes(typeof(DefaultValueAttribute), true);

				if (attributes.Length > 0)
				{
					DefaultValueAttribute defaultVal = (DefaultValueAttribute)attributes[0];

					if (value == null && defaultVal.Value == null)
						continue;

					if (value.Equals(defaultVal.Value))
						continue;
				}

				if (!TypeDescriptor.GetConverter(info.PropertyType).CanConvertTo(typeof(string)))
					continue;

				this.list[info.Name] = new NameValue(info.Name, value);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertiesCollection"/> class.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The streaming context.</param>
		private PropertiesCollection(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			this.list = (Dictionary<string, NameValue>)info.GetValue(NAMEVALUE, typeof(Dictionary<string, NameValue>));
		}

		#endregion

		#region Properties

		/// <summary>Gets the number of elements contained in the <see cref="PropertiesCollection"/>.</summary>
		/// <value>The number of elements contained in the <see cref="PropertiesCollection"/>.</value>
		public int Count
		{
			get { return this.list.Count; }
		}

		/// <summary>
		/// Gets or sets the class.
		/// </summary>
		/// <value>
		/// The class.
		/// </value>
		public string Class
		{
			get { return this.klass; }
			set { this.klass = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>Gets or sets the <see cref="System.Object"/> with the specified key.</summary>
		/// <param name="key">The key.</param>
		/// <returns>The value.</returns>
		public object this[string key]
		{
			get { return this.list[key].Value; }
			set { this.Set(key, value); }
		}

		/// <summary>
		/// Assigns the properties to an object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="obj"/> is null.</exception>
		/// <exception cref="T:System.InvalidOperationException">obj does not have a property in the PropertiesCollection or the property is read-only.</exception>
		public virtual void AssignTo(object obj)
		{
			if (obj == null)
				throw new ArgumentNullException("object");

			Type objType = obj.GetType();
			PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(objType);

			foreach (NameValue element in this)
				element.AssignTo(obj, descriptors);
		}

		/// <summary>
		/// Sets the specified property name and value in the <see cref="PropertiesCollection"/>.
		/// </summary>
		/// <param name="name">The property name.</param>
		/// <param name="value">The property value.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="name"/> is null or empty.</exception>
		/// <exception cref="T:System.NotSupportedException">The <see cref="PropertiesCollection"/> is read-only.</exception>
		public virtual void Set(string name, object value)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			this.list[name] = new NameValue(name, value);
		}

		/// <summary>
		/// Removes all items from the <see cref="PropertiesCollection"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">The <see cref="PropertiesCollection"/> is read-only. </exception>
		public void Clear()
		{
			this.list.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="PropertiesCollection"/> contains a specific property.
		/// </summary>
		/// <param name="name">The property name to locate in the <see cref="PropertiesCollection"/>.</param>
		/// <returns><c>true</c> if <paramref name="name"/> is found in the <see cref="PropertiesCollection"/>; otherwise, <c>false</c>.</returns>
		public bool Contains(string name)
		{
			return this.list.ContainsKey(name);
		}

		/// <summary>
		/// Copies the elements of the <see cref="PropertiesCollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="PropertiesCollection"/>.
		/// The <see cref="T:System.Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		public void CopyTo(NameValue[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (arrayIndex >= array.Length)
				throw new ArgumentOutOfRangeException("arrayIndex");

			foreach (NameValue value in this.list.Values)
			{
				if (arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException();

				array[arrayIndex++] = value;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific property from the <see cref="PropertiesCollection" />.
		/// </summary>
		/// <param name="name">The name of the property to remove from the <see cref="PropertiesCollection" />.</param>
		/// <returns>
		///   <c>true</c> if <paramref name="name" /> was successfully removed from the <see cref="PropertiesCollection" />; otherwise, false.
		/// This method also returns false if <paramref name="item" /> is not found in the original <see cref="PropertiesCollection" />.
		/// </returns>
		public bool Remove(string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			return this.list.Remove(name);
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
		public IEnumerator<NameValue> GetEnumerator()
		{
			return this.list.Values.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.list.Values.GetEnumerator();
		}

		/// <summary>
		/// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (Nothing in Visual Basic) from this method,
		/// and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/> to the class.
		/// </summary>
		/// <returns>An <see cref="T:System.Xml.Schema.XmlSchema"/> that describes the XML representation of the object that is produced by the
		/// <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> method and consumed by the
		/// <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> method.</returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> stream from which the object is deserialized.</param>
		public virtual void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;

			this.Clear();

			if (wasEmpty)
				return;

			this.klass = reader.GetAttribute(CLASS_ATTRIBUTE);

			reader.Read();
			XmlRootAttribute xroot      = new XmlRootAttribute(PROPERTY_TAG);
			XmlSerializer    serializer = new XmlSerializer(typeof(NameValue), xroot);

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				NameValue property = serializer.Deserialize(reader) as NameValue;

				if (property != null)
					this.list[property.Name] = property;
			}

			if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
				reader.Read();
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter"/> stream to which the object is serialized.</param>
		public virtual void WriteXml(XmlWriter writer)
		{
			if (string.IsNullOrEmpty(this.klass))
				writer.WriteAttributeString("class", this.Class);

			XmlRootAttribute xroot      = new XmlRootAttribute(PROPERTY_TAG);
			XmlSerializer    serializer = new XmlSerializer(typeof(NameValue), xroot);

			foreach (NameValue property in this)
				serializer.Serialize(writer, property);
		}

		/// <summary>
		/// Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo"/> with the data needed to serialize the target object.
		/// </summary>
		/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> to populate with data.</param>
		/// <param name="context">The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext"/>) for this serialization.</param>
		/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue(NAMEVALUE, this.list);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public object Clone()
		{
			PropertiesCollection result = new PropertiesCollection();

			foreach (NameValue element in this)
				result.list[element.Name] = (NameValue)element.Clone();

			return result;
		}

		#endregion
	}
}
