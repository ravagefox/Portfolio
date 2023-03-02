using System.Reflection;
using System.Xml;

namespace Perfekt.Core.IO.XML
{
#pragma warning disable CS8601, CS8618
    public sealed class XmlParser : IDisposable
    {
        public XmlNode Current { get; private set; }


        private XmlNode? rootNode;
        private Stream underlyingStream;
        private XmlDocument document;


        public XmlParser(Stream underlyingStream)
        {
            this.underlyingStream = underlyingStream;

            this.document = new XmlDocument();
            this.document.Load(underlyingStream);

            this.rootNode = document.FirstChild;
            this.Current = rootNode;
        }

        public object CreateInstance(XmlNode node)
        {
            var type = FindTypes(node.Name);
            if (type.Any() && type.FirstOrDefault() is Type t)
            {
#pragma warning disable CS8603 // Possible null reference return.

                return Activator.CreateInstance(t);
#pragma warning restore CS8603 // Possible null reference return.
            }

            return new object();
        }

        public object ReadValueType(XmlNode node)
        {
            var f = BindingFlags.Public | BindingFlags.Instance;

            var instance = CreateInstance(node);
            var fields = instance.GetType().GetFields(f);

            foreach (XmlNode child in node.ChildNodes)
            {
                if (fields.FirstOrDefault(f => f.Name.SequenceEqual(child.Name)) is FieldInfo inf)
                {
                    object? objValue = null;
                    try
                    {
                        objValue = Convert.ChangeType(child.InnerText, inf.FieldType);
                    }
                    catch
                    {
                        if (inf.FieldType.IsValueType && !inf.FieldType.IsPrimitive)
                        {
                            objValue = ReadValueType(child);
                        }
                    }

                    if (objValue != null)
                    {
                        inf.SetValue(instance, objValue);
                    }
                }
            }


            return instance;
        }

        public bool ResetReader()
        {
            if (Current == rootNode)
            {
                return false;
            }

            Current = rootNode;
            return Current != null && Current != rootNode;
        }

        public bool Read()
        {
            if (Current.HasChildNodes)
            {
                Current = Current.FirstChild;
            }
            else
            {
                if (Current.NextSibling != null)
                {
                    Current = Current.NextSibling;
                }
                else
                {
                    Current = Current.ParentNode?.NextSibling;
                }
            }
            return Current != null;
        }

        public void Dispose()
        {
            underlyingStream?.Dispose();
        }


        private IEnumerable<Type> FindTypes(string name)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in asms)
            {
                foreach (var type in asm.GetExportedTypes())
                {
                    if (type.Name.SequenceEqual(name))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
