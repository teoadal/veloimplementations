using System;
using System.IO;
using Velo.Serialization.Models;
using Velo.Serialization.Tokenization;

namespace Velo.Serialization.Converters
{
    internal sealed class EnumConverter<TEnum> : IJsonConverter<TEnum>
        where TEnum : Enum
    {
        public bool IsPrimitive => true;

        private readonly TEnum[] _values;

        public EnumConverter()
        {
            _values = (TEnum[]) Enum.GetValues(typeof(TEnum));
        }

        public TEnum Deserialize(JsonTokenizer tokenizer)
        {
            var token = tokenizer.Current;
            return _values[int.Parse(token.Value)];
        }

        public TEnum Read(JsonData jsonData)
        {
            var jsonValue = (JsonValue) jsonData;
            return _values[int.Parse(jsonValue.Value)];
        }

        public void Serialize(TEnum value, TextWriter writer)
        {
            var index = Array.IndexOf(_values, value);
            writer.Write(index);
        }

        public JsonData Write(TEnum value)
        {
            var index = Array.IndexOf(_values, value);
            return new JsonValue(index.ToString(), JsonDataType.Number);
        }

        object IJsonConverter.DeserializeObject(JsonTokenizer tokenizer) => Deserialize(tokenizer);
        
        object IJsonConverter.ReadObject(JsonData data) => Read(data);

        void IJsonConverter.SerializeObject(object value, TextWriter writer) => Serialize((TEnum) value, writer);

        JsonData IJsonConverter.WriteObject(object value) => Write((TEnum) value);
    }
}