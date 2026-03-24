using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientHTTP.QueryBuilder.QueryMetadata
{
    public interface Method
    {
        public abstract string GetCommand();
    }
}
