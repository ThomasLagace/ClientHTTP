using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class Index
    {
        public string Path { get; init; }

        public Index(string path) 
        {
            Path = path;
        }
    }
}
