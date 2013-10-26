using System;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Giparus.Data.Connector.Mongo
{
    public class DateTimeStringSerializer : BsonBaseSerializer
    {
        #region Fields
        // ReSharper disable once InconsistentNaming
        private static readonly DateTimeStringSerializer _instance;
        #endregion


        #region Properties
        public static DateTimeStringSerializer Instance { get { return _instance; } }
        #endregion


        #region .ctor & Initializers
        internal DateTimeStringSerializer() { }

        static DateTimeStringSerializer()
        {
            _instance = new DateTimeStringSerializer();
        }
        #endregion


        public override object Deserialize(BsonReader bsonReader, Type nominalType, Type actualType, IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(DateTime));
            var stringDateTime = bsonReader.ReadString();
            var value = DateTime.Parse(stringDateTime);
            return value;
        }

        public override void Serialize(BsonWriter bsonWriter, Type nominalType, object value, IBsonSerializationOptions options)
        {
            var dateTime = (DateTime)value;
            var stringDateTime = dateTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
            bsonWriter.WriteString(stringDateTime);
        }
    }
}