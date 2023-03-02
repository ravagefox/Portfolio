namespace Perfekt.Core.IO.Json
{
    public sealed class JsonSerializeAsAttribute : Attribute
    {
        public string SerializedValue { get; }

        public bool SerializeAsArray { get; set; }



        public JsonSerializeAsAttribute(string v)
        {
            this.SerializedValue = v;
        }

        public JsonSerializeAsAttribute()
        {
            this.SerializeAsArray = false;
            this.SerializedValue = string.Empty;
        }
    }
}