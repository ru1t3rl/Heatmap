using System;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;

namespace Ru1t3rl.StateRecorder.Utilities.Json
{
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            var jsonObject = new JObject
            {
                { nameof(Vector3.x), value.x },
                { nameof(Vector3.y), value.y },
                { nameof(Vector3.z), value.z }
            };
            jsonObject.WriteTo(writer);
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            return new(
                (float)jsonObject[nameof(Vector3.x)],
                (float)jsonObject[nameof(Vector3.y)],
                (float)jsonObject[nameof(Vector3.z)]
            );
        }
    }
}