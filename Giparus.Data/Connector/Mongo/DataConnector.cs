using System;
using Giparus.Data.Model;
using Giparus.Data.Model.MongoModel;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Giparus.Data.Connector.Mongo
{
    using Model.MongoModel;

    public class DataConnector
    {
        #region Properties
        public static DataConnector Instance { get; set; }

        public MongoClient Client { get; private set; }
        public MongoServer Server { get; private set; }
        public MongoDatabase Database { get; private set; }
        #endregion


        #region Collections
        public MongoCollection<Node> Nodes { get { return this.Database.GetCollection<Node>("Node"); } }
        public MongoCollection<Way> Ways { get { return this.Database.GetCollection<Way>("Way"); } }
        public MongoCollection<Relation> Relations { get { return this.Database.GetCollection<Relation>("Relation"); } }
        #endregion


        #region .ctor

        static DataConnector()
        {
            Instance = new DataConnector();
            Instance.Initialize();
        }

        private DataConnector()
        {
            this.Client = new MongoClient("mongodb://localhost");
            this.Server = this.Client.GetServer();
            this.Database = Server.GetDatabase("OsmDb");
        }

        private void Initialize()
        {
            this.EnsureIndex();
            this.RegisterIdGenerator();
        }

        private void RegisterIdGenerator()
        {
            BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);
            BsonSerializer.RegisterSerializer(typeof(DateTime), DateTimeStringSerializer.Instance);

            BsonClassMap.RegisterClassMap<NodeBase>(entity =>
            {
                entity.AutoMap();
                entity.UnmapProperty(c => c.ChangeSetId);
                entity.UnmapProperty(c => c.Srid);
                entity.UnmapProperty(c => c.User);
                entity.UnmapProperty(c => c.UserId);
                entity.UnmapProperty(c => c.Visible);
                entity.UnmapProperty(c => c.TimeStamp);
            });

            BsonClassMap.RegisterClassMap<Node>(entity =>
            {
                entity.AutoMap();
                entity.UnmapProperty(c => c.Latitude);
                entity.UnmapProperty(c => c.Longtitude);
            });
        }

        private void EnsureIndex()
        {
            this.Nodes.EnsureIndex("Id");
            this.Ways.EnsureIndex("Id");
            this.Relations.EnsureIndex("Id");
        }
        #endregion


        #region Partial Updates
        public void UpdateLatitudeLongtitude(Node node)
        {
            var query = Query<Node>.EQ(e => e.Id, node.Id);
            var update = Update<Node>.Set(e => e.LatLong, node.LatLong); // update modifiers
            this.Nodes.Update(query, update);
        }
        #endregion


        internal void InsertNewNode(Node node)
        {
            this.Nodes.Insert(node);
        }

        //public IEnumerable<PqItem> GetSpecificPqItems(string deviceId, string starTime, string endTime, string start, string end)
        //{
        //    var query = Query.And(new[]
        //    {
        //        //Query.And(new []
        //        //{
        //        //    Query<PqItem>.EQ(e => e.DeviceId, deviceId),
        //        //}),
        //        Query<PqItem>.EQ(e => e.DeviceId, deviceId),
        //        Query<PqItem>.GTE(e => e.Date, starTime),
        //        Query<PqItem>.LTE(e => e.Date, endTime),
        //    });

        //    return this.PqItems.Find(query);
        //}
    }
}
