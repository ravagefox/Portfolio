using System.Text;

namespace Perfekt.Core.IO
{
    public class CsvReader : IDisposable
    {

        private StreamReader streamReader;
        private string[] columnNames;


        public CsvReader(Stream srcStream)
        {
            streamReader = new StreamReader(srcStream, Encoding.ASCII, leaveOpen: true);
            columnNames = Array.Empty<string>();

            ReadColumns();
        }

        public void ReadColumns()
        {
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            columnNames = ReadNextLine();
        }

        public string[] ReadNextLine()
        {
            if (streamReader.EndOfStream)
            {
                return Array.Empty<string>();
            }

            var line = streamReader.ReadLine();
            if (string.IsNullOrEmpty(line))
            {
                throw new NullReferenceException(nameof(line) + " was null when read.");
            }

            return line.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        }

        public T ReadToObject<T>()
            where T : class, new()
        {
            var line = ReadNextLine();
            var t = new T();

            for (var i = 0; i < columnNames.Length; i++)
            {
                object value;

                var propertyInfo = typeof(T).GetProperty(columnNames[i]);
                if (propertyInfo == null)
                {
                    var fieldInfo = typeof(T).GetField(columnNames[i]);
                    if (fieldInfo != null)
                    {
                        value = Convert.ChangeType(line[i], fieldInfo.FieldType);
                        fieldInfo.SetValue(t, value);
                    }
                }
                else
                {
                    if (propertyInfo.SetMethod != null)
                    {
                        value = Convert.ChangeType(line[i], propertyInfo.PropertyType);
                        propertyInfo.SetValue(t, value);
                    }
                }
            }

            return t;
        }


        public void Dispose()
        {
            streamReader?.Dispose();

            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            streamReader?.Close();
        }
    }
}