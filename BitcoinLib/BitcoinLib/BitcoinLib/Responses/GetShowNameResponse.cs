using System;
using System.Collections.Generic;
using System.Text;

namespace BitcoinLib.Responses
{
    // Doesn't follow Bitcoinlib naming convention - because of daemon return values
    public class GetShowNameResponse
    {
        public string name { get; set; }
        public string name_encoding { get; set; }
        public string name_error { get; set; }
        public string value { get; set; }
        public string value_encoding { get; set; }
        public string txid { get; set; }
        public int vout { get; set; }
        public string address { get; set; }
        public bool ismine { get; set; }
        public int height { get; set; }
    }
}
