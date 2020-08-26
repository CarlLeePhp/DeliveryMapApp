using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.Database.SQLite
{
    public class ContactSQLite
    {
        [PrimaryKey, Unique]
        public string ContactID { get; set; }
        public string Fullname { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public ContactType Type { get; set; }
        public double? Latitude { get; set; } = null;
        public double? Longitude { get; set; } = null;
    }

    public enum ContactType
    {
        Customer,
        Supplier
    }
}
