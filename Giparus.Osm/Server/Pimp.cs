using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Giparus.Data.Connector.Mongo;
using System.Diagnostics;
using Giparus.Data.Model.MongoModel;
using Giparus.Data.Model.SqlModel;
using Microsoft.SqlServer.Types;

namespace Giparus.Osm.Server
{
    public class Pimp
    {
        #region Fields
        private readonly DataConnector _mongoConnector;
        private readonly long _localChangeset;
        private readonly SqlConnection _db;
        #endregion

        #region .ctor
        public Pimp()
        {
            _mongoConnector = DataConnector.Instance;
            _localChangeset = 1;
            const string connectionString = "Initial Catalog=osmdb;Data Source=Hecarim\\MainServer;Integrated Security=true;";
            _db = new SqlConnection(connectionString);
        }
        #endregion

        public void UpdateNodes()
        {
            var nodes = _mongoConnector.GetAllNodesFromChangeset(_localChangeset);
            _db.Open();

            foreach (var node in nodes)
            {
                try
                {
                    this.ProcessNode(node);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            _db.Close();
        }

        public void UpdatNodesInBulk()
        {
            _db.Open();

            Console.WriteLine("--------------- Updating Nodes ---------------");

            var nodes = _mongoConnector.GetAllNodesFromChangeset(_localChangeset);
            var tempNodeList = new List<Node>();

            foreach (var node in nodes)
            {
                tempNodeList.Add(node);

                if (tempNodeList.Count != 10000) continue;

                var nodeTable = CreateNodeTable();
                var nodeTagTable = CreateTagTable();
                foreach (var node1 in tempNodeList)
                {
                    foreach (var tagKey in node1.Tags.Keys)
                    {
                        var tagRow = nodeTagTable.NewRow();
                        var value = node1.Tags[tagKey];
                        var validValue = value.Substring(0, Math.Min(200, value.Length));
                        tagRow.ItemArray = new object[] { node1.Id, tagKey, validValue };
                        nodeTagTable.Rows.Add(tagRow);
                    }

                    var sqlNode = new SqlNode
                    {
                        Id = node1.Id,
                        TimeStamp = node1.TimeStamp,
                        LocalChangeset = node1.LocalChangeset,
                        Latitude = node1.Latitude,
                        Longtitude = node1.Longtitude,
                    };

                    sqlNode.MakePosition();

                    var nodeRow = nodeTable.NewRow();
                    nodeRow.ItemArray = new object[]
                    {
                        sqlNode.Id, sqlNode.TimeStamp, sqlNode.LocalChangeset, sqlNode.Latitude, sqlNode.Longtitude,
                        sqlNode.Position
                    };

                    nodeTable.Rows.Add(nodeRow);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "Node";
                    bulk.WriteToServer(nodeTable);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "NodeTag";
                    bulk.WriteToServer(nodeTagTable);
                }

                tempNodeList = new List<Node>(10000);
                Console.WriteLine("10000 Passed");
            }

            //----------------------------------------------------------------------------
            // Remains
            var nodeTable1 = CreateNodeTable();
            var nodeTagTable1 = CreateTagTable();

            foreach (var node1 in tempNodeList)
            {
                foreach (var tagKey in node1.Tags.Keys)
                {
                    var tagRow = nodeTagTable1.NewRow();
                    var value = node1.Tags[tagKey];
                    var validValue = value.Substring(0, Math.Min(200, value.Length));
                    tagRow.ItemArray = new object[] { node1.Id, tagKey, validValue };
                    nodeTagTable1.Rows.Add(tagRow);
                }

                var sqlNode = new SqlNode
                {
                    Id = node1.Id,
                    TimeStamp = node1.TimeStamp,
                    LocalChangeset = node1.LocalChangeset,
                    Latitude = node1.Latitude,
                    Longtitude = node1.Longtitude,
                };

                sqlNode.MakePosition();

                var nodeRow = nodeTable1.NewRow();
                nodeRow.ItemArray = new object[]
                    {
                        sqlNode.Id, sqlNode.TimeStamp, sqlNode.LocalChangeset, sqlNode.Latitude, sqlNode.Longtitude,
                        sqlNode.Position
                    };

                nodeTable1.Rows.Add(nodeRow);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "Node";
                bulk.WriteToServer(nodeTable1);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "NodeTag";
                bulk.WriteToServer(nodeTagTable1);
            }

            _db.Close();
        }

        public void UpdatWaysInBulk()
        {
            _db.Open();

            Console.WriteLine("--------------- Updating Ways ---------------");

            var ways = _mongoConnector.GetAllWaysFromChangeset(_localChangeset);
            var tempWayList = new List<Way>();

            foreach (var way in ways)
            {
                tempWayList.Add(way);

                if (tempWayList.Count != 10000) continue;

                var wayTable = CreateWayTable();
                var wayTagTable = CreateTagTable();
                foreach (var way1 in tempWayList)
                {
                    foreach (var tagKey in way1.Tags.Keys)
                    {
                        var tagRow = wayTagTable.NewRow();
                        var value = way1.Tags[tagKey];
                        var validValue = value.Substring(0, Math.Min(200, value.Length));
                        tagRow.ItemArray = new object[] { way1.Id, tagKey, validValue };
                        wayTagTable.Rows.Add(tagRow);
                    }

                    var sqlWay = new SqlWay
                    {
                        Id = way1.Id,
                        TimeStamp = way1.TimeStamp,
                        LocalChangeset = way1.LocalChangeset,
                        NodeIds = way1.NodeIds,
                    };

                    sqlWay.MakeWay(_mongoConnector.Nodes);

                    var wayRow = wayTable.NewRow();
                    wayRow.ItemArray = new object[]
                    {
                        sqlWay.Id, (byte) sqlWay.Shape, sqlWay.TimeStamp, sqlWay.LocalChangeset, sqlWay.Lines
                    };

                    wayTable.Rows.Add(wayRow);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "Way";
                    bulk.WriteToServer(wayTable);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "WayTag";
                    bulk.WriteToServer(wayTagTable);
                }

                tempWayList = new List<Way>(10000);
                Console.WriteLine("10000 Passed");
            }

            //----------------------------------------------------------------------------
            // Remains
            var wayTable1 = CreateWayTable();
            var wayTagTable1 = CreateTagTable();
            foreach (var way1 in tempWayList)
            {
                foreach (var tagKey in way1.Tags.Keys)
                {
                    var tagRow = wayTagTable1.NewRow();
                    var value = way1.Tags[tagKey];
                    var validValue = value.Substring(0, Math.Min(200, value.Length));
                    tagRow.ItemArray = new object[] { way1.Id, tagKey, validValue };
                    wayTagTable1.Rows.Add(tagRow);
                }

                var sqlWay = new SqlWay
                {
                    Id = way1.Id,
                    TimeStamp = way1.TimeStamp,
                    LocalChangeset = way1.LocalChangeset,
                    NodeIds = way1.NodeIds,
                };

                sqlWay.MakeWay(_mongoConnector.Nodes);

                var wayRow = wayTable1.NewRow();
                wayRow.ItemArray = new object[]
                    {
                        sqlWay.Id, (byte)sqlWay.Shape, sqlWay.TimeStamp, sqlWay.LocalChangeset, sqlWay.Lines
                    };

                wayTable1.Rows.Add(wayRow);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "Way";
                bulk.WriteToServer(wayTable1);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "WayTag";
                bulk.WriteToServer(wayTagTable1);
            }

            _db.Close();
        }

        public void UpdateRelationsInBulk()
        {
            _db.Open();

            Console.WriteLine("--------------- Updating Relations ---------------");

            var relations = _mongoConnector.GetAllRelationsFromChangeset(_localChangeset);
            var tempRelationList = new List<Relation>();

            foreach (var relation in relations)
            {
                tempRelationList.Add(relation);

                if (tempRelationList.Count != 10000) continue;

                var relationTable = CreateRelationTable();
                var relationTagTable = CreateTagTable();
                var memberTable = CreatememberTable();

                foreach (var relation1 in tempRelationList)
                {
                    foreach (var tagKey in relation1.Tags.Keys)
                    {
                        var tagRow = relationTagTable.NewRow();
                        var value = relation1.Tags[tagKey];
                        var validValue = value.Substring(0, Math.Min(200, value.Length));
                        tagRow.ItemArray = new object[] { relation1.Id, tagKey, validValue };
                        relationTagTable.Rows.Add(tagRow);
                    }

                    var memberReference = new HashSet<long>();
                    foreach (var member in relation1.Members)
                    {
                        if (memberReference.Contains(member.Reference)) continue;
                        memberReference.Add(member.Reference);

                        var memberRow = relationTagTable.NewRow();
                        var validValue = member.Role.Substring(0, Math.Min(200, member.Role.Length));
                        memberRow.ItemArray = new object[] { relation1.Id, member.Reference, member.Type, validValue };
                        memberTable.Rows.Add(memberRow);
                    }

                    var sqlRelation = new SqlRelation
                    {
                        Id = relation1.Id,
                        TimeStamp = relation1.TimeStamp,
                        LocalChangeset = relation1.LocalChangeset,
                    };


                    var relationRow = relationTable.NewRow();
                    relationRow.ItemArray = new object[]
                    {
                        sqlRelation.Id, sqlRelation.TimeStamp, sqlRelation.LocalChangeset
                    };

                    relationTable.Rows.Add(relationRow);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "Relation";
                    bulk.WriteToServer(relationTable);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "RelationTag";
                    bulk.WriteToServer(relationTagTable);
                }

                using (var bulk = new SqlBulkCopy(_db))
                {
                    bulk.DestinationTableName = "Member";
                    bulk.WriteToServer(memberTable);
                }

                tempRelationList = new List<Relation>(10000);
                Console.WriteLine("10000 Passed");
            }

            //----------------------------------------------------------------------------
            // Remains
            var relationTable1 = CreateRelationTable();
            var relationTagTable1 = CreateTagTable();
            var memberTable1 = CreatememberTable();

            foreach (var relation1 in tempRelationList)
            {
                foreach (var tagKey in relation1.Tags.Keys)
                {
                    var tagRow = relationTagTable1.NewRow();
                    var value = relation1.Tags[tagKey];
                    var validValue = value.Substring(0, Math.Min(200, value.Length));
                    tagRow.ItemArray = new object[] { relation1.Id, tagKey, validValue };
                    relationTagTable1.Rows.Add(tagRow);
                }

                var memberReference = new HashSet<long>();
                foreach (var member in relation1.Members)
                {
                    if (memberReference.Contains(member.Reference)) continue;
                    memberReference.Add(member.Reference);

                    var memberRow = memberTable1.NewRow();
                    var validValue = member.Role.Substring(0, Math.Min(200, member.Role.Length));
                    memberRow.ItemArray = new object[] { relation1.Id, member.Reference, member.Type, validValue };
                    memberTable1.Rows.Add(memberRow);
                }

                var sqlRelation = new SqlRelation
                {
                    Id = relation1.Id,
                    TimeStamp = relation1.TimeStamp,
                    LocalChangeset = relation1.LocalChangeset,
                };

                var relationRow = relationTable1.NewRow();
                relationRow.ItemArray = new object[]
                    {
                        sqlRelation.Id, sqlRelation.TimeStamp, sqlRelation.LocalChangeset
                    };

                relationTable1.Rows.Add(relationRow);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "Relation";
                bulk.WriteToServer(relationTable1);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "RelationTag";
                bulk.WriteToServer(relationTagTable1);
            }

            using (var bulk = new SqlBulkCopy(_db))
            {
                bulk.DestinationTableName = "Member";
                bulk.WriteToServer(memberTable1);
            }

            _db.Close();
        }

        private static DataTable CreateRelationTable()
        {
            var relationTable = new DataTable("Relation");
            relationTable.Columns.Add("Id", typeof(long));
            relationTable.Columns.Add("TimeStamp", typeof(DateTime));
            relationTable.Columns.Add("LocalChangeset", typeof(long));
            return relationTable;
        }

        private static DataTable CreatememberTable()
        {
            var memberTable = new DataTable("Member");
            memberTable.Columns.Add("ReferenceId", typeof(long));
            memberTable.Columns.Add("RelationId", typeof(long));
            memberTable.Columns.Add("NodeType", typeof(byte));
            memberTable.Columns.Add("Role", typeof(string));
            return memberTable;
        }

        private static DataTable CreateWayTable()
        {
            var wayTable = new DataTable("Way");
            wayTable.Columns.Add("Id", typeof(long));
            wayTable.Columns.Add("NodeType", typeof(byte));
            wayTable.Columns.Add("TimeStamp", typeof(DateTime));
            wayTable.Columns.Add("LocalChangeset", typeof(long));
            wayTable.Columns.Add("Lines", typeof(SqlGeography));
            return wayTable;
        }

        private static DataTable CreateTagTable()
        {
            var nodeTagTable = new DataTable("Tag");
            nodeTagTable.Columns.Add("Id", typeof(long));
            nodeTagTable.Columns.Add("Key", typeof(string));
            nodeTagTable.Columns.Add("Value", typeof(string));
            return nodeTagTable;
        }

        private static DataTable CreateNodeTable()
        {
            var nodeTable = new DataTable("Node");
            nodeTable.Columns.Add("Id", typeof(long));
            nodeTable.Columns.Add("TimeStamp", typeof(DateTime));
            nodeTable.Columns.Add("LocalChangeset", typeof(long));
            nodeTable.Columns.Add("Latitude", typeof(double));
            nodeTable.Columns.Add("Longtitude", typeof(double));
            nodeTable.Columns.Add("Position", typeof(SqlGeography));
            return nodeTable;
        }

        private void ProcessNode(Node node)
        {
            var sqlNode = new SqlNode
            {
                Id = node.Id,
                TimeStamp = node.TimeStamp,
                LocalChangeset = node.LocalChangeset,
                Latitude = node.Latitude,
                Longtitude = node.Longtitude,
            };

            sqlNode.MakePosition();

            var command = new SqlCommand("Insert into Node (Id, TimeStamp, Latitude, Longtitude, Position) values (@Id, @TimeStamp, @Latitude, @Longtitude, @Position)")
            {
                Connection = _db
            };

            command.Parameters.Add(new SqlParameter("@Id", sqlNode.Id));
            command.Parameters.Add(new SqlParameter("@TimeStamp", sqlNode.TimeStamp));
            command.Parameters.Add(new SqlParameter("@LocalChangeset", sqlNode.LocalChangeset));
            command.Parameters.Add(new SqlParameter("@Latitude", sqlNode.Latitude));
            command.Parameters.Add(new SqlParameter("@Longtitude", sqlNode.Longtitude));
            command.Parameters.Add(new SqlParameter("@Position", sqlNode.Position) { UdtTypeName = "Geography" });

            command.ExecuteNonQuery();
        }
    }
}
