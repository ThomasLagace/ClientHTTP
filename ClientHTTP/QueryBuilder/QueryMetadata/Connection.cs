using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class Connection
    {
        public string Behaviour { get; init; }

        public Connection(string behaviour)
        {
            Behaviour = behaviour;
        }
    }
}
