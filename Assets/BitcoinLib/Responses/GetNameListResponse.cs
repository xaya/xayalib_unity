﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BitcoinLib.Responses
{
    public class GetNameListResponse
    {
        public string name { get; set; }
        public string name_encoding { get; set; }
        public string value  { get; set; }
        public string value_encoding { get; set; }
        public string txid { get; set; }
        public int vout { get; set; }
        public string address { get; set; }
        public bool ismine { get; set; }
        public int height { get; set; }
    }
}
