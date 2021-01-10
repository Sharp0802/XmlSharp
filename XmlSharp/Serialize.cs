using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;

namespace XmlSharp
{
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

                    e.SetAttribute("value", p.GetValue(obj).ToString());
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
                pt.SetValue(obj, Convert.ChangeType(node.GetAttribute("value"), pt.PropertyType));
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
        internal static string BytesToString(byte[] bytes) => Encoding.UTF8.GetString(bytes);
        internal static byte[] StringToBytes(string str) => Encoding.UTF8.GetBytes(str);

        public static object BytesToObject(byte[] arrBytes)
        {
            using var memStream = new MemoryStream();
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = binForm.Deserialize(memStream);
                return obj;
        }
        public static byte[] ObjectToBytes(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using var ms = new MemoryStream();
                bf.Serialize(ms, obj);
                return ms.ToArray();
        }
    }
}
