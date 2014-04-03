// Filename: ExtensionInfoCollection.cs
// Author:   Jucardi
// Date:     3/23/2014 06:14:18 PM
//
// The use of this code is subject to Open Source License Agreement.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Jucardi.FlexibleService.Common.Extensions;

namespace Jucardi.FlexibleService.Common.Collections
{
	/// <summary>
	/// Extensions information collection.
	/// </summary>
	[Serializable, XmlRoot("extensions-collection")]
	public class ExtensionInfoCollection : IEnumerable<ExtensionInfo>, ISerializable, IXmlSerializable, ICloneable
	{
		#region Constants

		private const string ITEM = "add";

		#endregion

		#region Fields

		private Dictionary<string, ExtensionInfo> list = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfoCollection"/> class.
		/// </summary>
		public ExtensionInfoCollection()
		{
			this.list = new Dictionary<string, ExtensionInfo>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfoCollection"/> class.
		/// </summary>
		/// <param name="copy">The collection to copy from.</param>
		public ExtensionInfoCollection(ExtensionInfoCollection copy)
		{
			if (copy == null)
				throw new ArgumentNullException("copy");

			this.list = new Dictionary<string, ExtensionInfo>();

			foreach (KeyValuePair<string, ExtensionInfo> item in copy.list)
				this.list.Add(item.Key, item.Value);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExtensionInfoCollection"/> class.
		/// </summary>
		/// <param name="info">The serialization information.</param>
		/// <param name="context">The streaming context.</param>
		protected ExtensionInfoCollection(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
					throw new ArgumentNullException("info");

			this.list = (Dictionary<string, ExtensionInfo>)info.GetValue(ITEM, typeof(Dictionary<string, ExtensionInfo>));
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the number of elements contained in the <see cref="ExtensionInfoCollection"/>.
		/// </summary>
		/// <value>The number of elements contained in the <see cref="ExtensionInfoCollection"/>.</value>
		public int Count
		{
			get { return this.list.Count; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ExtensionInfo"/> with the specified name.
		/// </summary>
		/// <param name="name">The device name.</param>
		/// <returns>The ExtensionInfo with the specified name.</returns>
		public ExtensionInfo this[string name]
		{
			get
			{
				return this.list[name];
			}

			set
			{
				if (value == null)
				{
					this.list.Remove(name);
					return;
				}

				this.list[name] = value;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="info">The info.</param>
		public void Add(string name, ExtensionInfo info)
		{
			this.list[name] = info;
		}

		/// <summary>
		/// Removes the first occurrence of a specific property from the <see cref="ExtensionInfoCollection" />.
		/// </summary>
		/// <param name="name">The name of the property to remove from the <see cref="ExtensionInfoCollection" />.</param>
		/// <returns>
		///   <c>true</c> if <paramref name="name" /> was successfully removed from the <see cref="ExtensionInfoCollection" />; otherwise, false.
		/// This method also returns false if <paramref name="item" /> is not found in the original <see cref="ExtensionInfoCollection" />.
		/// </returns>
		public bool Remove(string name)
		{
			return this.list.Remove(name);
		}

		/// <summary>
		/// Removes all items from the <see cref="ExtensionInfoCollection"/>.
		/// </summary>
		public void Clear()
		{
			this.list.Clear();
		}

		/// <summary>
		/// Determines whether the <see cref="ExtensionInfoCollection"/> contains a specific property.
		/// </summary>
		/// <param name="name">The device name to locate in the <see cref="ExtensionInfoCollection"/>.</param>
		/// <returns><c>true</c> if <paramref name="name"/> is found in the <see cref="ExtensionInfoCollection"/>; otherwise, <c>false</c>.</returns>
		public bool Contains(string name)
		{
			return this.list.ContainsKey(name);
		}

		/// <summary>
		/// Copies the elements of the <see cref="ExtensionInfoCollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="ExtensionInfoCollection"/>.
		/// The <see cref="T:System.Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
		public void CopyTo(ExtensionInfo[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException("array");

			if (arrayIndex >= array.Length)
				throw new ArgumentOutOfRangeException("arrayIndex");

			foreach (ExtensionInfo info in this.list.Values)
			{
				if (arrayIndex >= array.Length)
					throw new ArgumentOutOfRangeException();

				array[arrayIndex++] = info;
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>A <see cref="IEnumerator"/> that can be used to iterate through the collection.</returns>
		public IEnumerator<ExtensionInfo> GetEnumerator()
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
		/// Gets the schema.
		/// </summary>
		/// <returns>The xml schema</returns>
		public XmlSchema GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Reads the XML.
		/// </summary>
		/// <param name="reader">The xml reader.</param>
		public void ReadXml(XmlReader reader)
		{
			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			this.Clear();

			if (wasEmpty)
				return;

			XmlRootAttribute xroot = new XmlRootAttribute(ITEM);
			XmlSerializer serializer = new XmlSerializer(typeof(ExtensionInfo), xroot);

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
			{
				ExtensionInfo info = serializer.Deserialize(reader) as ExtensionInfo;

				if (info != null)
					this.list[info.Name] = info;
			}

			if (reader.NodeType == System.Xml.XmlNodeType.EndElement)
				reader.Read();
		}

		/// <summary>
		/// Writes the XML.
		/// </summary>
		/// <param name="writer">The xml writer.</param>
		public void WriteXml(XmlWriter writer)
		{
			XmlRootAttribute xroot     = new XmlRootAttribute(ITEM);
			XmlSerializer    serialzer = new XmlSerializer(typeof(ExtensionInfo), xroot);

			foreach (ExtensionInfo info in this)
				serialzer.Serialize(writer, info);
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

			info.AddValue(ITEM, this.list);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public object Clone()
		{
			ExtensionInfoCollection result = new ExtensionInfoCollection();

			foreach (KeyValuePair<string, ExtensionInfo> item in this.list)
				result.list.Add(item.Key, (ExtensionInfo)item.Value.Clone());

			return result;
		}

		#endregion
	}
}
