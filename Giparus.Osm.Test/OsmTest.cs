using System;
using System.Diagnostics;
using Giparus.Data.Connector.Mongo;
using Giparus.Osm.Importer;
using Giparus.Osm.Server;

namespace Giparus.Osm.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OsmTest
    {
        [TestMethod]
        public void TestReadOsmXmlFile()
        {
            var stopWatch = Stopwatch.StartNew();

            const string file = @"E:\Research\Maps\Map Files\geofabrik.de.asia\iran-latest.osm";
            var reader = new OsmXmlMongoReader();
            reader.Open(file);
            reader.Read();

            stopWatch.Stop();
            var totalTime1 = stopWatch.ElapsedMilliseconds;

            stopWatch = Stopwatch.StartNew();

            DataConnector.Instance.Nodes.InsertBatch(reader.Nodes.Values);
            DataConnector.Instance.Ways.InsertBatch(reader.Ways.Values);
            DataConnector.Instance.Relations.InsertBatch(reader.Relations.Values);

            stopWatch.Stop();
            var totalTime2 = stopWatch.ElapsedMilliseconds;


            Debug.WriteLine("Step1 Done in {0} ms", totalTime1);
            Debug.WriteLine("Step2 Done in {0} ms", totalTime2);
        }

        [TestMethod]
        public void TestReadOsmXmlFileAndInsert()
        {
            var stopWatch = Stopwatch.StartNew();

            const string file = @"E:\Research\Maps\Map Files\geofabrik.de.asia\iran-latest.osm";
            var reader = new OsmXmlMongoReader();
            reader.Open(file);
            reader.ReadAndInsertInBulk();

            stopWatch.Stop();
            var totalTime = stopWatch.ElapsedMilliseconds;
            Debug.WriteLine("Done in {0} ms", totalTime);
        }

        [TestMethod]
        public void TestReadOsmXmlFileAndInsertWithUpdate()
        {
            var stopWatch = Stopwatch.StartNew();

            const string file = @"E:\Research\Maps\Map Files\geofabrik.de.asia\iran-latest.osm";
            var reader = new OsmXmlMongoReader();
            reader.Open(file);
            reader.ReadAndInsertAndCheck();

            TestFillSqlFromMongo();

            stopWatch.Stop();
            var totalTime = stopWatch.ElapsedMilliseconds;
            Debug.WriteLine("Done in {0} ms", totalTime);
        }

        [TestMethod]
        public void TestFillSqlFromMongo()
        {
            var start = DateTime.Now;

            var pimp = new Pimp();

            // in 389.0 s
            pimp.UpdatNodesInBulk();
            var time1 = DateTime.Now.Subtract(start).TotalMilliseconds;

            // in 821.0 s
            pimp.UpdatWaysInBulk();
            var time2 = DateTime.Now.Subtract(start).TotalMilliseconds - time1;

            // in 0.778 s
            pimp.UpdateRelationsInBulk();
            var time3 = DateTime.Now.Subtract(start).TotalMilliseconds - time2;

            Debug.WriteLine("Updating nodes in {0} ms", time1);
            Debug.WriteLine("Updating ways in {0} ms", time2);
            Debug.WriteLine("Updating relations in {0} ms", time3);

            var totalTime = DateTime.Now.Subtract(start).TotalMilliseconds;
            Debug.WriteLine("Done in {0} ms", totalTime);
        }

        internal void RunSomeQueries()
        {
            var start = DateTime.Now;



            var totalTime = DateTime.Now.Subtract(start).TotalMilliseconds;
            Debug.WriteLine("Done in {0} ms", totalTime);
        }
    }
}
