using System;
using System.Runtime.Remoting.Messaging;
using Giparus.Data.Connector.Mongo;

namespace Giparus.Data.Model
{
    public class Global
    {
        #region Properties
        public string Id { get; set; }
        public DateTime Time { get; set; }

        public long Value { get; set; }
        #endregion


        public Global GetGlobalValue(string key)
        {
            var entity = DataConnector.Instance.Globals.FindOneById(key);
            return entity;
        }


        #region .ctor
        public Global() { }
        #endregion

    }
}
