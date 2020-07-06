using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MySql.Data.MySqlClient;
using SQLite;
using DeliveryAppWhiterocks.Data;

namespace DeliveryAppWhiterocks.Models
{
    public class User
    {
        [PrimaryKey]
        public int ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public User() { }
        public User(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        public User(int id,string username, string password)
        {
            this.ID = id;
            this.Username = username;
            this.Password = password;
        }

        public bool checkInformation()
        {
            if (this.Username != "" && this.Password != "")
            {
                User tempUser = DatabaseController.retrieveUserDataLocal(this);

                if (tempUser == null && App.CheckIfInternet().Result)
                {
                    tempUser = DatabaseController.retrieveUserDataOnline(this);
                }

                if (tempUser != null)
                {
                    this.ID = tempUser.ID;
                    this.Password = tempUser.Password;
                    this.Username = tempUser.Username;
                    return true;
                } 
            }
            return false;
        }

        
    }
}
