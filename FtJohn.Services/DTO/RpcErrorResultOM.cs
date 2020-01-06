using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FtJohn.Services.DTO
{
    public class RpcErrorResultOM
    {
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
}
