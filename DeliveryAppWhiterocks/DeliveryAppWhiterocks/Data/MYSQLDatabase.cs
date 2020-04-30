using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;

namespace DeliveryAppWhiterocks.Data
{
    class MYSQLDatabase
    {
        //private const string CSTRING = @"Server=db4free.net;Port=3306;Database=deliveryapp;Uid=deliveryguy;Pwd=DeliveryFeed1;";
        private const string CSTRING = @"Server=65.19.141.67;Port=3306;Database=nzdas123_deliveryDatabase;Uid=nzdas123_deliveryPerson;Pwd=DeliveryFeed1;";

        public bool validateUser(string username,string password)
        {
            string sql = "SELECT UserID,UserName, UserPassword from deliveryUser WHERE UserName = @USERNAME AND UserPassword = @PASSWORD";
            using(MySqlConnection conn = new MySqlConnection(CSTRING))
            {
                conn.Open();
                using(MySqlCommand command = new MySqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("USERNAME",username);
                    command.Parameters.AddWithValue("PASSWORD", password);
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        return true;
                    }
                    
                }
                conn.Close();
            }
            return false;
        }
    }
}
