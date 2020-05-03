using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DeliveryAppWhiterocks.Data;
using DeliveryAppWhiterocks.Droid.Data;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLiteAndroid))]
namespace DeliveryAppWhiterocks.Droid.Data
{
    public class SQLiteAndroid : ISQLite
    {
        public SQLiteAndroid() { }

        public SQLite.SQLiteConnection GetConnection(){

            var sqliteFile = "DeliveryUserDB.db3";
            string documentPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            
            var path = Path.Combine(documentPath,sqliteFile);
            var conn = new SQLite.SQLiteConnection(path);

            return conn;
        }
    }
}