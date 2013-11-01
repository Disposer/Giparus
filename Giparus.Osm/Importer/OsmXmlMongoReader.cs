using System.Collections.Generic;
using System.Linq;
using Giparus.Data.Connector.Mongo;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Giparus.Osm.Importer
{
    using Data.Model;
    using Data.Model.MongoModel;


    public class OsmXmlMongoReader : IOsmXmlReader
    {
        #region Fields
        private readonly DataConnector _connector;
        private XmlReader _reader;

        public Dictionary<long, INode> Nodes;
        public Dictionary<long, IWay> Ways;
        public Dictionary<long, Relation> Relations;

        public Dictionary<string, bool> NodeTags { get; private set; }
        public Dictionary<string, bool> WayTags { get; private set; }
        public Dictionary<string, bool> RelationTags { get; private set; }
        #endregion


        #region .ctor
        public OsmXmlMongoReader()
        {
            _connector = DataConnector.Instance;
        }
        #endregion

        public void Open(string path)
        {
            _reader = XmlReader.Create(path);

            this.NodeTags = new Dictionary<string, bool>();
            this.WayTags = new Dictionary<string, bool>();
            this.RelationTags = new Dictionary<string, bool>();

            this.Nodes = new Dictionary<long, INode>();
            this.Ways = new Dictionary<long, IWay>();
            this.Relations = new Dictionary<long, Relation>();
        }

        public void Read()
        {
            _reader.MoveToContent();
            _reader.ReadStartElement("osm");
            _reader.Skip();

            if (_reader.Name == "bound")
            {
                _reader.ReadStartElement("bound");
                _reader.Skip();
            }

            while (_reader.IsStartElement())
            {
                switch (_reader.Name)
                {
                    case "node":
                        var node = ReadNode(_reader);
                        this.Nodes.Add(node.Id, node);
                        break;
                    case "way":
                        var way = ReadWay(_reader);
                        this.Ways.Add(way.Id, way);
                        break;
                    case "relation":
                        var relation = ReadRelation(_reader);
                        this.Relations.Add(relation.Id, relation);
                        break;
                    default:
                        _reader.Skip();
                        break;
                }
            }
        }

        public void ReadAndInsertInBulk()
        {
            _reader.MoveToContent();
            _reader.ReadStartElement("osm");
            _reader.Skip();

            if (_reader.Name == "bound")
            {
                _reader.ReadStartElement("bound");
                _reader.Skip();
            }

            var nodeList = new List<Node>(100000);
            var wayList = new List<Way>(1000);
            var relationList = new List<Relation>(10000);

            var firstWay = true;
            var firstRelation = true;
            while (_reader.IsStartElement())
            {
                switch (_reader.Name)
                {
                    case "node":
                        var node = ReadNode(_reader);
                        nodeList.Add(node);
                        if (nodeList.Count == 100000)
                        {
                            _connector.InsertBatch(nodeList);
                            nodeList = new List<Node>(100000);
                        }
                        break;
                    case "way":
                        if (firstWay)
                        {
                            _connector.InsertBatch(nodeList);
                            firstWay = false;
                        }
                        var way = ReadWayAndNodes(_reader, _connector);
                        wayList.Add(way);
                        if (wayList.Count == 1000)
                        {
                            _connector.InsertBatch(wayList);
                            wayList = new List<Way>(1000);
                        }
                        break;
                    case "relation":
                        if (firstRelation)
                        {
                            _connector.InsertBatch(wayList);
                            firstRelation = false;
                        }
                        var relation = ReadRelation(_reader);
                        relationList.Add(relation);
                        if (relationList.Count == 10000)
                        {
                            _connector.InsertBatch(relationList);
                            relationList = new List<Relation>(10000);
                        }
                        break;
                    default:
                        _reader.Skip();
                        break;
                }
            }

            _connector.InsertBatch(relationList);
        }

        public void ReadAndInsertAndCheck()
        {
            _reader.MoveToContent();
            _reader.ReadStartElement("osm");
            _reader.Skip();

            if (_reader.Name == "bound")
            {
                _reader.ReadStartElement("bound");
                _reader.Skip();
            }

            var nodeList = new List<Node>(100000);
            var wayList = new List<Way>(1000);
            var relationList = new List<Relation>(10000);

            var firstWay = true;
            var firstRelation = true;
            var localChangeset = _connector.UpdateChangeset();

            while (_reader.IsStartElement())
            {
                switch (_reader.Name)
                {
                    case "node":
                        var node = this.ReadNode(_reader);
                        node.LocalChangeset = localChangeset;

                        var entity = _connector.GetNodeChangeset(node.Id);
                        if (entity == null)
                        {
                            nodeList.Add(node);
                            if (nodeList.Count != 100000) continue;

                            _connector.InsertBatch(nodeList);
                            nodeList = new List<Node>(100000);
                        }
                        else
                        {
                            if (entity.ChangesetId == node.ChangesetId) continue;
                            _connector.UpdateNode(node);
                        }
                        break;
                    case "way":
                        if (firstWay)
                        {
                            _connector.InsertBatch(nodeList);
                            firstWay = false;
                        }
                        var way = this.ReadWayAndNodes(_reader, _connector);
                        way.LocalChangeset = localChangeset;

                        var wayEntity = _connector.GetWayChangeset(way.Id);
                        if (wayEntity == null)
                        {
                            wayList.Add(way);
                            if (nodeList.Count != 1000) continue;

                            _connector.InsertBatch(wayList);
                            wayList = new List<Way>(1000);
                        }
                        else
                        {
                            if (wayEntity.ChangesetId == way.ChangesetId) continue;
                            _connector.UpdateWay(way);
                        }
                        break;
                    case "relation":
                        if (firstRelation)
                        {
                            _connector.InsertBatch(wayList);
                            firstRelation = false;
                        }
                        var relation = this.ReadRelation(_reader);
                        relation.LocalChangeset = localChangeset;

                        var relationEntity = _connector.GetRelationChangeset(relation.Id);
                        if (relationEntity == null)
                        {
                            relationList.Add(relation);
                            if (nodeList.Count != 10000) continue;

                            _connector.InsertBatch(nodeList);
                            relationList = new List<Relation>(10000);
                        }
                        else
                        {
                            if (relationEntity.ChangesetId == relation.ChangesetId) continue;
                            _connector.UpdateRelation(relation);
                        }
                        break;
                    default:
                        _reader.Skip();
                        break;
                }
            }

            _connector.InsertBatch(relationList);
        }


        private Node ReadNode(XmlReader reader)
        {
            var nodeElement = XNode.ReadFrom(reader) as XElement;
            if (nodeElement == null) throw new InvalidOperationException("Empty node");

            var latitude = double.Parse(nodeElement.Attribute("lat").Value);
            var longtitude = double.Parse(nodeElement.Attribute("lon").Value);

            var node = ReadNodeBase<Node>(nodeElement);
            node.Tags = ReadNodeTags(nodeElement);

            node.MakePosition(latitude, longtitude);
            reader.Skip();

            return node;
        }

        private Way ReadWayAndNodes(XmlReader reader, DataConnector connector)
        {
            var nodeElement = XNode.ReadFrom(reader) as XElement;
            if (nodeElement == null) throw new InvalidOperationException("Empty node");

            var way = ReadNodeBase<Way>(nodeElement);
            way.Tags = ReadNodeTags(nodeElement);
            way.NodeIds = ReadWayNodeIds(nodeElement);

            way.MakeWay(connector);

            reader.Skip();

            return way;
        }

        private Way ReadWay(XmlReader reader)
        {
            var nodeElement = XNode.ReadFrom(reader) as XElement;
            if (nodeElement == null) throw new InvalidOperationException("Empty node");

            var way = ReadNodeBase<Way>(nodeElement);
            way.Tags = ReadNodeTags(nodeElement);
            way.NodeIds = ReadWayNodeIds(nodeElement);

            way.MakeWay(Nodes);

            reader.Skip();

            return way;
        }

        private Relation ReadRelation(XmlReader reader)
        {
            var nodeElement = XNode.ReadFrom(reader) as XElement;
            if (nodeElement == null) throw new InvalidOperationException("Empty node");

            var node = ReadNodeBase<Relation>(nodeElement);
            node.Tags = ReadNodeTags(nodeElement);
            node.Members = ReadRelationMembers(nodeElement);

            reader.Skip();

            return node;
        }

        private static T ReadNodeBase<T>(XElement node) where T : NodeBase
        {
            var instance = (T)Activator.CreateInstance(typeof(T), true);

            var id = node.Attribute("id").Value;
            var version = node.Attribute("version").Value;
            var timestamp = node.Attribute("timestamp").Value;
            var uid = node.Attribute("uid").Value;
            var user = node.Attribute("user").Value;
            var changesetId = node.Attribute("changeset").Value;

            instance.Type = NodeType.Node;
            instance.Id = long.Parse(id);
            instance.Version = string.IsNullOrEmpty(version) ? 0 : int.Parse(version);
            instance.TimeStamp = string.IsNullOrEmpty(timestamp) ? DateTime.MinValue : DateTime.Parse(timestamp);
            instance.UserId = string.IsNullOrEmpty(uid) ? 0 : long.Parse(uid);
            instance.User = user;
            instance.ChangesetId = string.IsNullOrEmpty(changesetId) ? 0 : long.Parse(changesetId);

            return instance;
        }

        private static TagCollection ReadNodeTags(XElement node)
        {
            var tagCollection = new TagCollection();
            foreach (var tagElement in node.Elements("tag"))
            {
                var key = tagElement.Attribute("k").Value;
                var value = tagElement.Attribute("v").Value;

                tagCollection.Add(key, value);
            }

            return tagCollection;
        }

        private static IList<Member> ReadRelationMembers(XElement node)
        {
            var members = new List<Member>();
            foreach (var memberElement in node.Elements("member"))
            {
                var type = memberElement.Attribute("type").Value;
                var reference = memberElement.Attribute("ref").Value;
                var role = memberElement.Attribute("role").Value;

                var nodeType = NodeType.Undefined;
                switch (type)
                {
                    case "node":
                        nodeType = NodeType.Node;
                        break;
                    case "way":
                        nodeType = NodeType.Way;
                        break;
                    case "relation":
                        nodeType = NodeType.Relation;
                        break;
                }

                var member = new Member
                {
                    Type = nodeType,
                    Reference = string.IsNullOrEmpty(reference) ? 0 : long.Parse(reference),
                    Role = role,
                };

                members.Add(member);
            }

            return members;
        }

        private static IList<long> ReadWayNodeIds(XElement node)
        {
            return node.Elements("nd").Select(tagElement => long.Parse(tagElement.Attribute("ref").Value)).ToList();
        }
    }
}
