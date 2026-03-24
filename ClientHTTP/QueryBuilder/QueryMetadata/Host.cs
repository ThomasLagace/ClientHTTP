using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class Host
    {
        public string Domain { get; init; }

        public Host(string domain) 
        {
            Domain = domain;
        }
    }
}
