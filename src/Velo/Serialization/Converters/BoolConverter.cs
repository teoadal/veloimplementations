using System.IO;
using Velo.Serialization.Models;
using Velo.Serialization.Tokenization;
using Velo.Utils;

namespace Velo.Serialization.Converters
{
    internal sealed class BoolConverter : IJsonConverter<bool>
    {
        public bool IsPrimitive => true;

        public bool Deserialize(JsonTokenizer tokenizer)
        {
            var token = tokenizer.Current;

            switch (token.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                default:
                    throw Error.InvalidOperation($"Invalid boolean token '{token.TokenType}'");
            }
        }

        public bool Read(JsonData jsonData)
        {
            var jsonValue = (JsonValue) jsonData;
            return jsonValue.Type == JsonDataType.True;
        }

        public void Serialize(bool value, TextWriter writer)
        {
            writer.Write(value ? JsonValue.TrueToken : JsonValue.FalseToken);
        }

        public JsonData Write(bool value)
        {
            return value ? JsonValue.True : JsonValue.False;
        }

        object IJsonConverter.DeserializeObject(JsonTokenizer tokenizer) => Deserialize(tokenizer);

        object IJsonConverter.ReadObject(JsonData data) => Read(data);

        void IJsonConverter.SerializeObject(object value, TextWriter writer) => Serialize((bool) value, writer);

        JsonData IJsonConverter.WriteObject(object value) => Write((bool) value);
    }
}