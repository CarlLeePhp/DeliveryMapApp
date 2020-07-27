using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryAppWhiterocks.Models.XeroAPI
{
    public class Contact
    {
        public string ContactID { get; set; }
        public string ContactStatus { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string BankAccountDetails { get; set; }
        public string TaxNumber { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Phone> Phones { get; set; }
        public DateTime UpdatedDateUTC { get; set; }
        public List<ContactGroup> ContactGroups { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsCustomer { get; set; }
        public string DefaultCurrency { get; set; }
        public List<Tracking> SalesTrackingCategories { get; set; }
        public List<Tracking> PurchasesTrackingCategories { get; set; }
        public BatchPayment BatchPayments { get; set; }
        public List<ContactPerson> ContactPersons { get; set; }
        public bool HasValidationErrors { get; set; }

        
    }
}
