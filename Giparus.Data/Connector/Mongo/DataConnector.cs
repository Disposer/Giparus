using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
        public MongoCollection<Global> Globals { get { return this.Database.GetCollection<Global>("Global"); } }
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
                //entity.UnmapProperty(c => c.ChangeSetId);
                entity.UnmapProperty(c => c.Srid);
                entity.UnmapProperty(c => c.User);
                entity.UnmapProperty(c => c.UserId);
                entity.UnmapProperty(c => c.Visible);
                //entity.UnmapProperty(c => c.TimeStamp);
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
            //this.Nodes.EnsureIndex("Id");
            this.Nodes.EnsureIndex(IndexKeys.GeoSpatialSpherical("LatLong"));
            //this.Ways.EnsureIndex("Id");
            //this.Relations.EnsureIndex("Id");
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


        public void InsertNode(Node node)
        {
            this.Nodes.Insert(node);
        }
        public void InsertBatch(List<Node> nodes)
        {
            if (nodes.Count == 0) return;
            this.Nodes.InsertBatch(nodes);
        }

        public void InsertWay(Way way)
        {
            this.Ways.Insert(way);
        }

        public void InsertBatch(List<Way> ways)
        {
            if (ways.Count == 0) return;
            this.Ways.InsertBatch(ways);
        }

        public void InsertRelation(Relation relation)
        {
            this.Relations.Insert(relation);
        }
        public void InsertBatch(List<Relation> relations)
        {
            if (relations.Count == 0) return;
            this.Relations.InsertBatch(relations);
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

        public Node GetSpecificNode(long nodeIndex)
        {
            //var query = Query.And(new[]
            //{                
            //    Query<Node>.EQ(e => e.Id, nodeIndex),
            //});

            return this.Nodes.FindOneById(nodeIndex);
        }

        public Node GetNodeChangeset(long index)
        {
            var result = this.Nodes.FindOneById(index);
            return result;
        }

        public Way GetWayChangeset(long index)
        {
            var result = this.Ways.FindOneById(index);
            return result;
        }

        public Relation GetRelationChangeset(long index)
        {
            var result = this.Relations.FindOneById(index);
            return result;
        }


        public IEnumerable<Node> GetSpecificNodes(IList<long> nodeIndecies)
        {
            var query = Query<Node>.In(e => e.Id, nodeIndecies);

            var limit = nodeIndecies.Count();
            return this.Nodes.Find(query).SetLimit(limit).ToArray();
        }

        public void UpdateLocalChangset(Global entity)
        {
            this.Globals.Save(entity);
        }

        public long UpdateChangeset()
        {
            const string key = "ChangeSet";
            var entity = this.Globals.FindOneById(key);
            if (entity == null)
            {
                this.Globals.Insert(new Global { Id = key, Time = DateTime.Now, Value = 1 });
                return 1;
            }

            entity.Value++;
            entity.Time = DateTime.Now;
            this.Globals.Save(entity);

            return entity.Value;
        }

        public void UpdateNode(Node entity)
        {
            this.Nodes.Save(entity);
        }

        public void UpdateWay(Way entity)
        {
            this.Nodes.Save(entity);
        }

        public void UpdateRelation(Relation entity)
        {
            this.Relations.Save(entity);
        }
    }
}
