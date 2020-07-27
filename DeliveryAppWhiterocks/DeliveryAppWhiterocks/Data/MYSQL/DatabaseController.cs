using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Data.MYSQL
{
    class DatabaseController
    {
        public static User retrieveUserDataLocal(User user)
        {
            User tempUser = App.UserDatabase.GetUser(user);

            return tempUser;
        }

        public static User retrieveUserDataOnline(User user)
        {
            User tempUser = MYSQLDatabase.retrieveUserInformation(user.Username, user.Password);
            if (tempUser != null)
            {
                if (tempUser.Username == user.Username && tempUser.Password == user.Password)
                {
                    return tempUser;
                }
            }
            return null;
        }
    }
}
