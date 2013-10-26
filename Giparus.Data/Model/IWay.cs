using System;
using System.Collections.Generic;
using Giparus.Data.Model.SqlModel;

namespace Giparus.Data.Model
{
    public interface IWay
    {
        IList<long> NodeIds { get; set; }
        void MakeWay(Dictionary<long, INode> nodes);
    }
}
