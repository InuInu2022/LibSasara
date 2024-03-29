// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using CevioCasts;
//
//    var definitions = Definitions.FromJson(jsonString);

namespace CevioCasts
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class Definitions
    {
        [JsonProperty("$schema")]
        public Uri Schema { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("casts")]
        public Cast[] Casts { get; set; }
    }

    public partial class Cast
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("cname")]
        public string Cname { get; set; }

        [JsonProperty("names")]
        public Name[] Names { get; set; }

        [JsonProperty("category")]
        public Category Category { get; set; }

        [JsonProperty("product")]
        public Product Product { get; set; }

        [JsonProperty("gender")]
        public Gender Gender { get; set; }

        [JsonProperty("lang")]
        public Lang Lang { get; set; }

        [JsonProperty("versions")]
        public string[] Versions { get; set; }

        [JsonProperty("hasEmotions")]
        public bool HasEmotions { get; set; }

        [JsonProperty("emotions", NullValueHandling = NullValueHandling.Ignore)]
        public Emotion[] Emotions { get; set; }

        [JsonProperty("spSymbols", NullValueHandling = NullValueHandling.Ignore)]
        public SpSymbol[] SpSymbols { get; set; }
    }

    public partial class Emotion
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("names")]
        public Name[] Names { get; set; }
    }

    public partial class Name
    {
        [JsonProperty("lang")]
        public Lang Lang { get; set; }

        [JsonProperty("display")]
        public string Display { get; set; }
    }

    public partial class SpSymbol
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbols")]
        public string[] Symbols { get; set; }

        [JsonProperty("names")]
        public Name[] Names { get; set; }
    }

    public enum Category { SingerSong, TextVocal };

    public enum Lang { English, Japanese };

    public enum Gender { Female, Male };

    public enum Product { CeVIO_AI, CeVIO_CS, VoiSona };

    public partial class Definitions
    {
        public static Definitions FromJson(string json) => JsonConvert.DeserializeObject<Definitions>(json, CevioCasts.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Definitions self) => JsonConvert.SerializeObject(self, CevioCasts.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                CategoryConverter.Singleton,
                LangConverter.Singleton,
                GenderConverter.Singleton,
                ProductConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class CategoryConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Category) || t == typeof(Category?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "SingerSong":
                    return Category.SingerSong;
                case "TextVocal":
                    return Category.TextVocal;
            }
            throw new Exception("Cannot unmarshal type Category");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Category)untypedValue;
            switch (value)
            {
                case Category.SingerSong:
                    serializer.Serialize(writer, "SingerSong");
                    return;
                case Category.TextVocal:
                    serializer.Serialize(writer, "TextVocal");
                    return;
            }
            throw new Exception("Cannot marshal type Category");
        }

        public static readonly CategoryConverter Singleton = new CategoryConverter();
    }

    internal class LangConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Lang) || t == typeof(Lang?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "English":
                    return Lang.English;
                case "Japanese":
                    return Lang.Japanese;
            }
            throw new Exception("Cannot unmarshal type Lang");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Lang)untypedValue;
            switch (value)
            {
                case Lang.English:
                    serializer.Serialize(writer, "English");
                    return;
                case Lang.Japanese:
                    serializer.Serialize(writer, "Japanese");
                    return;
            }
            throw new Exception("Cannot marshal type Lang");
        }

        public static readonly LangConverter Singleton = new LangConverter();
    }

    internal class GenderConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Gender) || t == typeof(Gender?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Female":
                    return Gender.Female;
                case "Male":
                    return Gender.Male;
            }
            throw new Exception("Cannot unmarshal type Gender");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Gender)untypedValue;
            switch (value)
            {
                case Gender.Female:
                    serializer.Serialize(writer, "Female");
                    return;
                case Gender.Male:
                    serializer.Serialize(writer, "Male");
                    return;
            }
            throw new Exception("Cannot marshal type Gender");
        }

        public static readonly GenderConverter Singleton = new GenderConverter();
    }

    internal class ProductConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Product) || t == typeof(Product?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "CeVIO AI":
                    return Product.CeVIO_AI;
                case "CeVIO CS":
                    return Product.CeVIO_CS;
                case "VoiSona":
                    return Product.VoiSona;
            }
            throw new Exception("Cannot unmarshal type Product");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Product)untypedValue;
            switch (value)
            {
                case Product.CeVIO_AI:
                    serializer.Serialize(writer, "CeVIO AI");
                    return;
                case Product.CeVIO_CS:
                    serializer.Serialize(writer, "CeVIO CS");
                    return;
                case Product.VoiSona:
                    serializer.Serialize(writer, "VoiSona");
                    return;
            }
            throw new Exception("Cannot marshal type Product");
        }

        public static readonly ProductConverter Singleton = new ProductConverter();
    }
}
