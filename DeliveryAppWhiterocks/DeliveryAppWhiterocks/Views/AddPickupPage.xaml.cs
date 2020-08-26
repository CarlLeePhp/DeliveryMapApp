using DeliveryAppWhiterocks.Models.Database.SQLite;
using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DeliveryAppWhiterocks.Models.XeroAPI;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPickupPage : ContentPage
    {
        List<string> contactNameList = new List<string>();
        List<ContactSQLite> contactsSQLite = new List<ContactSQLite>();

        public List<ItemSQLite> itemsSQLite = new List<ItemSQLite>();
        public ObservableCollection<LineItem> _itemsCollection = new ObservableCollection<LineItem>();

        public AddPickupPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            App.CheckInternetIfConnected(noInternetLbl, this);

            contactsSQLite = App.ContactDatabase.GetAllContact();

            foreach (ContactSQLite contact in contactsSQLite)
            {
                contactNameList.Add(string.Format("{0}, {1}", contact.Fullname, contact.City));
            }

            itemsSQLite = App.ItemDatabase.GetAllItems();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            ItemsListView.ItemsSource = _itemsCollection;

            if (_itemsCollection.Count > 0)
            {
                ItemsListView.IsVisible = true;
                DeleteAllItemButton.IsVisible = true;
            }
            else
            {
                ItemsListView.IsVisible = false;
                DeleteAllItemButton.IsVisible = false;
            }
        }

        private void ContactAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text != "")
            {
                sender.ItemsSource = contactNameList.Where(c => c.StartsWith(ContactAutoSuggestBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
        }

        private void ContactAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            if(sender.Text != "")
            {
                int index = contactNameList.IndexOf(sender.Text);
                if (index == -1) return;
                sender.Text = contactsSQLite[index].Fullname;
                AddressEntry.Text = contactsSQLite[index].Address;
                CityEntry.Text = contactsSQLite[index].City;
            }
        }

        private void ItemsListView_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if ((sender as ListView).ItemsSource != null)
                (sender as ListView).HeightRequest = ((sender as ListView).ItemsSource as ObservableCollection<LineItem>).Count * (sender as ListView).RowHeight+90;
        }

        private void AddItemButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddPickupItemPage(this));
        }

        private void DeleteAllItemButton_Clicked(object sender, EventArgs e)
        {
            _itemsCollection.Clear();
            ItemsListView.ItemsSource = _itemsCollection;
            ItemsListView.IsVisible = false;
            DeleteAllItemButton.IsVisible = false;
        }

        private void SavePickupButton_Clicked(object sender, EventArgs e)
        {
            if (!ValidateInput())
            {
                DisplayAlert("Alert","Please Fill All the Input Field","OK");
                return;
            }

            int pickupInvoiceCount = App.InvoiceDatabase.CountPickupInvoice();
            ContactSQLite contact = App.ContactDatabase.GetContactByName(ContactAutoSuggestBox.Text);

            if(contact == null)
            {
                contact = new ContactSQLite()
                {
                    ContactID = DateTime.Now.ToString(),
                    Fullname = ContactAutoSuggestBox.Text,
                    City = CityEntry.Text,
                    Type = ContactType.Supplier,
                    Address = AddressEntry.Text,
                };
                App.ContactDatabase.InsertContact(contact);
            }

            InvoiceSQLite invoice = new InvoiceSQLite()
            {
                InvoiceID = DateTime.Now.ToString(),
                InvoiceNumber = "OPC-" + pickupInvoiceCount,
                InvoiceType = "ACCPAY",
                CompletedDeliveryStatus = false,
                ContactID = contact.ContactID,
            };
            App.InvoiceDatabase.InsertInvoice(invoice);

            foreach(LineItem item in _itemsCollection)
            {
                var maxItemLineID = App.LineItemDatabase.GetLastLineItem();
                //create the id by referencing lineitemtable
                LineItemSQLite lineItemSQLite = new LineItemSQLite()
                {
                    // if it's not set set the itemline id to 1 else increment 1 from the biggest value
                    ItemLineID = (maxItemLineID == null ? 1 : maxItemLineID.ItemLineID + 1),
                    InvoiceID = invoice.InvoiceID,
                    ItemCode = item.ItemCode,
                    Quantity = (int)item.Quantity,
                    UnitAmount = item.UnitAmount,
                };
                //Save to db
                App.LineItemDatabase.InsertLineItem(lineItemSQLite);

                //check if item already exist, if not add it into database
                if (!App.ItemDatabase.CheckIfExisted(item.ItemCode))
                {
                    ItemSQLite newItem = new ItemSQLite()
                    {
                        ItemCode = item.ItemCode,
                        Description = item.Description,
                        Weight = item.Weight,
                        UnitCost = item.UnitAmount
                    };
                    App.ItemDatabase.InsertItem(newItem);
                }
            }

            Navigation.PopModalAsync();
        }

        private bool ValidateInput()
        {
            if(ContactAutoSuggestBox.Text == "" || CityEntry.Text == "" || AddressEntry.Text == "")
            {
                return false;
            } else
            {
                return true;
            }
        }

        private void ImgMenu_Tapped(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}