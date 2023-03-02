using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Perfekt.Core.IO.Json.JsonConverters
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to and from its name string value.
    /// And is adapted from the original <see cref="Newtonsoft.Json.Converters.StringEnumConverter"/>
    /// </summary>
    public class PerfektStringEnumConverter : JsonConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether the written enum text should be camel case.
        /// The default value is <c>false</c>.
        /// </summary>
        /// <value><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</value>
        [Obsolete("StringEnumConverter.CamelCaseText is obsolete. Set StringEnumConverter.NamingStrategy with CamelCaseNamingStrategy instead.")]
        public bool CamelCaseText
        {
            get => NamingStrategy is CamelCaseNamingStrategy ? true : false;
            set
            {
                if (value)
                {
                    if (NamingStrategy is CamelCaseNamingStrategy)
                    {
                        return;
                    }

                    NamingStrategy = new CamelCaseNamingStrategy();
                }
                else
                {
                    if (!(NamingStrategy is CamelCaseNamingStrategy))
                    {
                        return;
                    }

                    NamingStrategy = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the naming strategy used to resolve how enum text is written.
        /// </summary>
        /// <value>The naming strategy used to resolve how enum text is written.</value>
        public NamingStrategy? NamingStrategy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether integer values are allowed when serializing and deserializing.
        /// The default value is <c>true</c>.
        /// </summary>
        /// <value><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</value>
        public bool AllowIntegerValues { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfektStringEnumConverter"/> class.
        /// </summary>
        public PerfektStringEnumConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfektStringEnumConverter"/> class.
        /// </summary>
        /// <param name="camelCaseText"><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</param>
        [Obsolete("StringEnumConverter(bool) is obsolete. Create a converter with StringEnumConverter(NamingStrategy, bool) instead.")]
        public PerfektStringEnumConverter(bool camelCaseText)
        {
            if (camelCaseText)
            {
                NamingStrategy = new CamelCaseNamingStrategy();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerfektStringEnumConverter"/> class.
        /// </summary>
        /// <param name="namingStrategy">The naming strategy used to resolve how enum text is written.</param>
        /// <param name="allowIntegerValues"><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</param>
        public PerfektStringEnumConverter(NamingStrategy namingStrategy, bool allowIntegerValues = true)
        {
            NamingStrategy = namingStrategy;
            AllowIntegerValues = allowIntegerValues;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Enum e = (Enum)value;
            var v = GetAttributeOfType<JsonSerializeAsAttribute>(e);
            if (v != null)
            {
                if (!string.IsNullOrEmpty(v.SerializedValue) && !v.SerializeAsArray)
                {
                    writer.WriteValue(v.SerializedValue ?? e.ToString());
                    return;
                }
            }

            writer.WriteValue(e.ToString());
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string? enumText = reader.Value?.ToString();

                    if (string.IsNullOrEmpty(enumText)) { return null; }

                    var values = Enum.GetValues(objectType);
                    foreach (Enum v in values)
                    {
                        if (GetAttributeOfType<JsonSerializeAsAttribute>(v) is JsonSerializeAsAttribute attribute)
                        {
                            if (string.CompareOrdinal(enumText, attribute.SerializedValue) == 0)
                            {
                                return v;
                            }
                        }
                    }

                    return Enum.Parse(objectType, enumText);
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    return Convert.ChangeType(reader.Value, objectType);
                }

            }
            catch (Exception ex)
            {
                //throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(reader.Value), objectType), ex);
            }

            return null;
            // we don't actually expect to get here.
            //throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            var isNullable = (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>));
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Type t = (isNullable)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return t.IsEnum;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        /// <example><![CDATA[string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;]]></example>
        private static T GetAttributeOfType<T>(Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }
    }
}
