using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public class UserAgent
    {
        public string Browser { get; init; }

        public UserAgent(string browser) 
        {
            Browser = browser;
        }
    }
}
