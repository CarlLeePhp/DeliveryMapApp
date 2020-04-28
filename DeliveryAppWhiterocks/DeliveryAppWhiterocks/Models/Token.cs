using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models
{
    class Token
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public DateTime Expire_Date { get; set; }

        public Token() { }
    }
}
