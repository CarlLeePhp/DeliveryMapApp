using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace DeliveryAppWhiterocks.Data
{
    public interface ISQLite
    {
        SQLiteConnection GetConnection();
    }
}
