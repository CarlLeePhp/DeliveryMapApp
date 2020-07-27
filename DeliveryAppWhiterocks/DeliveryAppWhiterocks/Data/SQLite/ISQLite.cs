using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace DeliveryAppWhiterocks.Data.SQLite
{
    //An interface that will return the sqlite connection
    //Initialized on the Platform Specific android, DeliveryAppWhiterocks.Android.Data.SQLiteAndroid
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
