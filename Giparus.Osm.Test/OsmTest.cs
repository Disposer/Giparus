using System.Diagnostics;
using Giparus.Data.Connector.Mongo;
using Giparus.Data.Model;
using Giparus.Osm.Importer;
using ServiceStack.OrmLite;

namespace Giparus.Osm.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Data;

    [TestClass]
    public class OsmTest
    {
        private IDbConnection _database;

        [TestMethod]
        public void TestReadOsmXmlFile()
        {
            var stopWatch = Stopwatch.StartNew();
            //const string file = @"Map\Pasdaran.osm";
            const string file = @"E:\Research\Maps\Map Files\geofabrik.de.asia\iran-latest.osm";
            var reader = new OsmXmlReader();
            reader.Open(file);
            reader.Read();

            stopWatch.Stop();
            var s1 = stopWatch.ElapsedMilliseconds;


            stopWatch = Stopwatch.StartNew();
            //_database.CreateTable(false, typeof(Node));

            DataConnector.Instance.Nodes.InsertBatch(reader.Nodes.Values);
            DataConnector.Instance.Ways.InsertBatch(reader.Ways.Values);
            DataConnector.Instance.Relations.InsertBatch(reader.Relations.Values);

            //foreach (var nodeId in reader.Nodes.Keys)
            //{
            //    var node = reader.Nodes[nodeId];
            //    //_database.Insert(node);

            //    DataConnector.Instance.Nodes.InsertBatch(node);
            //}

            stopWatch.Stop();
            var s2 = stopWatch.ElapsedMilliseconds;
        }

        public void CreateDbConnection()
        {
            const string conectionString = "Initial Catalog=osmdb;Data Source=Hecarim\\MainServer;Integrated Security=true;";
            var factory = new OrmLiteConnectionFactory(conectionString, SqlServerDialect.Provider);
            _database = factory.OpenDbConnection();
        }
    }
}
