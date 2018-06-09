namespace StudyCSharp
{
    using System;
    using System.Diagnostics;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class UnixSecondsConverter : DateTimeConverterBase
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Debug.Assert(value is DateTime, "Expected date object value.");
            writer.WriteValue(UnixDateTimeHelper.DateTimeToUnixSeconds((DateTime)value).ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Debug.Assert(reader.Value != null);
            Debug.Assert(reader.TokenType == JsonToken.String, "Wrong Token Type");

            return UnixDateTimeHelper.UnixSecondsToDateTime((string)reader.Value);
        }
    }

    ///<summary>
    ///</summary>
    public class UnixDateTimeHelper
    {
        private static DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///   Convert a long seconds into a DateTime
        /// </summary>
        public static DateTime UnixSecondsToDateTime(int unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        /// <summary>
        ///   Convert a string seconds into a DateTime
        /// </summary>
        public static DateTime UnixSecondsToDateTime(string unixTime)
        {
            return epoch.AddSeconds(int.Parse(unixTime));
        }

        /// <summary>
        ///   Convert a DateTime into a long seconds.
        /// </summary>
        public static long DateTimeToUnixSeconds(DateTime dateTime)
        {
            Debug.Assert(dateTime.ToUniversalTime() >= epoch);

            var delta = dateTime.ToUniversalTime() - epoch;
            return (long)delta.TotalSeconds;
        }
    }
}
