using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;

namespace DeliveryAppWhiterocks.Models
{
    class User
    {

        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public User() { }
        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public bool checkInformation()
        {
            if (this.Username != "" && this.Password != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
