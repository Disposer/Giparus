using System.Collections.Generic;
using System.Linq;
using Giparus.Data.Model;
using System;
using System.Xml;
using System.Xml.Linq;

namespace Giparus.Osm.Importer
{
    using Data.Model;
    using Data.Model.MongoModel;

    public class OsmXmlReader
    {
        #region Fields
        private XmlReader _reader;

        public Dictionary<long, INode> Nodes;
        public Dictionary<long, IWay> Ways;
        public Dictionary<long, Relation> Relations;

        public Dictionary<string, bool> NodeTags { get; private set; }
        public Dictionary<string, bool> WayTags { get; private set; }
        public Dictionary<string, bool> RelationTags { get; private set; }
        #endregion


        #region .ctor
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

        private static Node ReadNode(XmlReader reader)
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

        private Way ReadWay(XmlReader reader)
        {
            var nodeElement = XNode.ReadFrom(reader) as XElement;
            if (nodeElement == null) throw new InvalidOperationException("Empty node");

            var way = ReadNodeBase<Way>(nodeElement);
            way.Tags = ReadNodeTags(nodeElement);
            way.NodeIds = ReadWayNodeIds(nodeElement);

            way.MakeWay(this.Nodes);

            reader.Skip();

            return way;
        }

        private static Relation ReadRelation(XmlReader reader)
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
            instance.ChangeSetId = string.IsNullOrEmpty(changesetId) ? 0 : long.Parse(changesetId);

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
