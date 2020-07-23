using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class TokenPrepare
    {
        public string grant_type { get; set; }
        public string client_id { get; set; }
        public string code { get; set; }
        public string redirect_uri { get; set; }
        public string code_verifier { get; set; }
    }
}
