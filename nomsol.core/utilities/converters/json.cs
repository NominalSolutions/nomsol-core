using System;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;
using System.Xml;

namespace nomsol.core.utilities.converters
{
    public static class json
    {
        /// <summary>
        /// Converts an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to convert to JSON.</param>
        /// <returns>A JSON string representing the object.</returns>
        public static string ObjToJson(this object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj), "Object cannot be null.");

            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Converts a JSON string to an object of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object of the specified type.</returns>
        public static T JsonToObj<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");

            return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Deserialization returned null.");
        }

        /// <summary>
        /// Converts an XML string to a JSON string.
        /// </summary>
        /// <param name="xml">The XML string to convert to JSON.</param>
        /// <returns>A JSON string representing the XML.</returns>
        public static string XMLToJson(this string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException(nameof(xml), "XML string cannot be null or empty.");

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            // Convert XmlDocument to an object and then to JSON
            using StringWriter sw = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(doc.DocumentElement.GetType());
            serializer.Serialize(sw, doc.DocumentElement);
            string intermediateJson = sw.ToString();

            // Since System.Text.Json doesn't support direct XML-to-JSON, we use an intermediate object
            var xmlReader = XmlReader.Create(new StringReader(xml));
            var xmlSerializer = new XmlSerializer(typeof(object), new XmlRootAttribute(doc.DocumentElement.Name));
            var obj = xmlSerializer.Deserialize(xmlReader);

            return JsonSerializer.Serialize(obj);
        }

        /// <summary>
        /// Converts a JSON string to an XML string.
        /// </summary>
        /// <param name="json">The JSON string to convert to XML.</param>
        /// <returns>An XML string representing the JSON.</returns>
        public static string JsonToXML(this string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(json), "JSON string cannot be null or empty.");

            // Deserialize JSON to an object
            object obj = JsonSerializer.Deserialize<object>(json) ?? throw new InvalidOperationException("Deserialization returned null.");

            // Serialize object to XML
            using StringWriter sw = new StringWriter();
            XmlSerializer serializer = new XmlSerializer(obj.GetType(), new XmlRootAttribute("Root"));
            serializer.Serialize(sw, obj);
            return sw.ToString();
        }

        /// <summary>
        /// Converts an XmlDocument to a JSON string.
        /// </summary>
        /// <param name="xmlDocument">The XmlDocument to convert to JSON.</param>
        /// <returns>A JSON string representing the XmlDocument.</returns>
        public static string XMLToJson(this XmlDocument xmlDocument)
        {
            if (xmlDocument == null)
                throw new ArgumentNullException(nameof(xmlDocument), "XmlDocument cannot be null.");

            return xmlDocument.OuterXml.XMLToJson();
        }
    }
}
