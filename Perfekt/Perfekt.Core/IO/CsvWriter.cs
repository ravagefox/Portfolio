using System.Reflection;
using System.Text;

namespace Perfekt.Core.IO
{
    public sealed class CsvWriter : IDisposable
    {
        public Stream BaseStream { get; }


        private StreamWriter streamWriter;
        private string[] columnNames;



        public CsvWriter(Stream destStream)
        {
            this.BaseStream = destStream;
            this.streamWriter = new StreamWriter(destStream, Encoding.ASCII, leaveOpen: true);
            this.columnNames = new string[0];
        }

        public void CreateColumns(Type type)
        {
            var f = BindingFlags.Public | BindingFlags.Instance;
            if (type.IsValueType)
            {
                var fieldInfos = type.GetFields(f);
                columnNames = fieldInfos.Select(f => f.Name).ToArray();
            }
            else
            {
                var propertyInfos = type.GetProperties(f);
                columnNames = propertyInfos.Select(f => f.Name).ToArray();
            }

            CreateColumns(columnNames);
        }

        public void CreateColumns(params string[] columnHeaders)
        {
            this.columnNames = columnHeaders;
            this.BaseStream.Seek(0, SeekOrigin.Begin);

            for (var i = 0; i < columnNames.Length; i++)
            {
                streamWriter.Write(columnNames[i]);
                streamWriter.Write(',');
            }

            streamWriter.Write(streamWriter.NewLine);
        }

        public void WriteLine(object instance)
        {
            var f = BindingFlags.Public | BindingFlags.Instance;

            var type = instance.GetType();
            if (type.IsValueType)
            {
                var fields = type.GetFields(f);
                if (fields.Length == columnNames.Length)
                {
                    WriteLine(fields.Select(f => f.GetValue(instance).ToString()).ToArray());
                }
            }
            else
            {
                var properties = type.GetProperties(f);
                if (properties.Length == columnNames.Length)
                {
                    WriteLine(properties.Select(f => f.GetValue(instance).ToString()).ToArray());
                }
            }
        }

        public void WriteLine(params string[] values)
        {
            if (values.Length != columnNames.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(values));
            }

            var builder = new StringBuilder();
            for (var i = 0; i < values.Length; i++)
            {
                builder.Append(values[i]);
                builder.Append(',');
            }

            streamWriter.WriteLine(builder.ToString());
        }



        public void Close()
        {
            this.streamWriter?.Close();
            this.BaseStream?.Close();
        }

        public void Dispose()
        {
            this.streamWriter?.Dispose();
            this.BaseStream?.Dispose();
        }
    }
}
