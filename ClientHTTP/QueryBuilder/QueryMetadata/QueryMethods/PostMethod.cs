using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata.QueryMethod
{
    public class PostMethod : Method
    {
        public string GetCommand() => "POST";
    }
}
