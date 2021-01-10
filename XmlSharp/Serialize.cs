using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;

namespace XmlSharp
{
    public class Program
    {
        public static Data data = new Data();

        public static void Main()
        {
            string dataxml = data.ToXmlFrom();
            Console.WriteLine(dataxml);
            data = (Data)dataxml.FromXmlTo();
            dataxml = data.ToXmlFrom();
            Console.WriteLine(dataxml);
        }
    }

    public class Data
    {
        [XmlSerializable]
        public string A { get; set; } = "가나다";
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlSerializable : Attribute { }

    public static class Methods
    {
        public static string ToXmlFrom(this object obj)
        {
            var docs = new XmlDocument();

            var root = docs.CreateElement("Object");
            root.SetAttribute("name", obj.GetType().FullName);
            docs.AppendChild(root);

            obj.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .AsParallel()
                .Where(p => p.GetCustomAttribute<XmlSerializable>() != null)
                .ForAll(p =>
                {
                    var e = docs.CreateElement("Property");
                    e.SetAttribute("name", p.Name);

                    string value = p
                        .GetValue(obj)
                        .ObjectToBytes()
                        .ToBase64();
                    e.SetAttribute("value", value);
                    root.AppendChild(e);
                });

            return docs.OuterXml;
        }
        public static object FromXmlTo(this string xml)
        {
            if (!ValidToDeserialize(xml))
                throw new XmlSerializeException();

            var docs = new XmlDocument();
            docs.LoadXml(xml);

            var xmlObj = (XmlElement)docs.GetElementsByTagName("Object")[0];

            object obj = Activator.CreateInstance(Type.GetType(xmlObj.GetAttribute("name")));

            foreach (var node in xmlObj.ChildNodes.Cast<XmlElement>())
            {
                var pt = obj.GetType().GetProperty(node.GetAttribute("name"));
                var value = node
                    .GetAttribute("value")
                    .FromBase64()
                    .BytesToObject();
                pt.SetValue(obj, value);
            }

            return obj;
        }
        public static bool ValidToDeserialize(this string xml)
        {
            var docs = new XmlDocument();
            docs.LoadXml(xml);

            var arr = docs.GetElementsByTagName("Object");
            return !(arr.Count != 1 ||
                    ((XmlElement)arr[0])
                            .GetAttribute("Object")
                            .Equals(null));
        }

        public class XmlSerializeException : Exception { }


        public static void Serialize(this string xml, string path)
        {
            File.WriteAllText(path, xml);
        }
        public static string Deserialize(this string path)
        {
            return File.ReadAllText(path);
        }
    }

    internal static class Convert
    {
        internal static object BytesToObject(this byte[] bytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(bytes, 0, bytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                return binForm.Deserialize(memStream);
            }
        }
        internal static byte[] ObjectToBytes(this object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        internal static string ToBase64(this byte[] bytes) => System.Convert.ToBase64String(bytes);
        internal static byte[] FromBase64(this string str) => System.Convert.FromBase64String(str);
    }
}
