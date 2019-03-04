using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinLib.Responses
{
    public class GetNamePendingResponse
    {
        public string name { get; set; }
        public string name_encoding { get; set; }
        public string value { get; set; }
        public string value_encoding { get; set; }
        public string txid { get; set; }
        public int vout { get; set; }
        public string address { get; set; }
        public bool ismine { get; set; }
        public string op { get; set; }
    }
}
