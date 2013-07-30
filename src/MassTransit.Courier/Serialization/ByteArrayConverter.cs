﻿// Copyright 2007-2013 Chris Patterson
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Courier.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Newtonsoft.Json;


    public class ByteArrayConverter :
        JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
                writer.WriteNull();
            else
            {
                byte[] byteArray = GetByteArray(value);
                writer.WriteValue(byteArray);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            byte[] numArray;
            if (reader.TokenType == JsonToken.StartArray)
                numArray = ReadByteArray(reader);
            else if (reader.TokenType == JsonToken.String)
                numArray = Convert.FromBase64String(reader.Value.ToString());
            else
            {
                throw new Exception(
                    string.Format("Unexpected token parsing binary. Expected String or StartArray, got {0}.",
                        reader.TokenType));
            }

            return numArray;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(byte[]))
                return true;

            return false;
        }

        byte[] GetByteArray(object value)
        {
            return value as byte[];
        }

        byte[] ReadByteArray(JsonReader reader)
        {
            var list = new List<byte>();
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        continue;
                    case JsonToken.Integer:
                        list.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
                        continue;
                    case JsonToken.EndArray:
                        return list.ToArray();
                    default:
                        throw new Exception(string.Format("Unexpected token when reading bytes: {0}", reader.TokenType));
                }
            }
            throw new Exception("Unexpected end when reading bytes.");
        }
    }
}