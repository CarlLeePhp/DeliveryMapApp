using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class AccessToken
    {
        public long nbf { get; set; }
        public long exp { get; set; }
        public string iss { get; set; }
        public string aud { get; set; }
        public string client_id { get; set; }
        public string sub { get; set; }
        public string auth_time { get; set; }
        public string xero_userid { get; set; }
        public string global_session_id { get; set; }
        public string jti { get; set; }
        public string authentication_event_id { get; set; }
        
    }
}
