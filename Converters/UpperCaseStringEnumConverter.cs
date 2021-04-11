using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace react_app.Converters
{
    public class UpperCaseStringEnumConverter : StringEnumConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                var sourceEnum = value as Enum;

                if (sourceEnum != null)
                {

                    var enumText = sourceEnum.ToString().ToUpper();
                    writer.WriteValue(enumText);
                }
            }
        }
    }
}
