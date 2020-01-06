using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtJohn.Services.DTO
{
    public class RpcCommonResultOM
    {
        public RpcCommonResultOM()
        {
            this.id = 1;
            this.jsonrpc = "2.0";
        }

        public object id { get; set; }
        public string jsonrpc { get; set; }
        public object result { get; set; }
        public RpcErrorResultOM error { get; set; }
    }
}
