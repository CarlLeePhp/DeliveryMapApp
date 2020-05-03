using DeliveryAppWhiterocks.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Data
{
    public class UserDatabaseController
    {
        static object locker = new object();

        SQLiteConnection database;

        public UserDatabaseController()
        {
            database = DependencyService.Get<ISQLite>().GetConnection();
            database.CreateTable<User>();
        }

        public User GetUser(User user)
        {
            lock (locker)
            {
                if (database.Table<User>().Count() == 0)
                {
                    return null;
                } else
                {
                    User tempUser = database.Table<User>().Where(savedData => savedData.Username == user.Username).FirstOrDefault();
                   
                    if (tempUser == null || tempUser.Password != user.Password)
                    {
                        return null;
                    }
                    else
                    {
                        return tempUser;
                    }
                }
            }
        }

        public User GetFirstUser()
        {
            lock (locker)
            {
                if(database.Table<User>().Count() == 0)
                {
                    return null;
                } else
                {
                    return database.Table<User>().First();
                }
            }
        }

        public int GetUserCount()
        {
            lock (locker)
            {
                return database.Table<User>().Count();
            }
        }

        public int SaveUser(User user)
        {
            lock (locker)
            {
                if (GetUser(user) != null)
                {
                    database.Update(user);
                    return user.ID;
                }
                else
                {
                    return database.Insert(user);
                }
            }
        }

        public int DeleteUser(int id)
        {
            lock (locker)
            {
                return database.Delete<User>(id);
            }
        }
    }
}
