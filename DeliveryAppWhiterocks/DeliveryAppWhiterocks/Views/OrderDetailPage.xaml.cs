using DeliveryAppWhiterocks.Data.SQLite;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OrderDetailPage : ContentPage
    {
        ObservableCollection<LineItem> _itemsCollection = new ObservableCollection<LineItem>();
        Invoice _selectedInvoice;

        public OrderDetailPage(Invoice selectedInvoice)
        {
            InitializeComponent();
            _selectedInvoice = selectedInvoice;
            Init();
            BindItemData();
        }

        private void Init()
        {
            if (_selectedInvoice.Status == "Completed")
            {
                PageHeaderLabel.Text = _selectedInvoice.InvoiceNumber + " Completed";
                CompletedOrderButton.Text = "Mark as incomplete";
            }
            else
            {
                PageHeaderLabel.Text = _selectedInvoice.InvoiceNumber;
            }

            PageHeaderLabel.Text += _selectedInvoice.Type == "ACCPAY" ? " - Pickup" : " - Dropoff";

            customerNameLabel.Text = _selectedInvoice.Contact.Name;
            if(_selectedInvoice.Contact.Addresses[1].AddressLine1 != "")
            {
                customerAddressLabel.Text = $"{_selectedInvoice.Contact.Addresses[1].AddressLine1} {_selectedInvoice.Contact.Addresses[1].City}";
            } else
            {
                addressLayout.IsVisible = false;
            }

            if(_selectedInvoice.Contact.Phones[1].PhoneNumber != "") { 
                phoneNumberLabel.Text = _selectedInvoice.Contact.Phones[1].PhoneNumber;
            } else
            {
                phoneLayout.IsVisible = false;
            }
            App.CheckInternetIfConnected(noInternetLbl, this);
        }

        private void BindItemData()
        {
            double itemsSubtotal = 0;
            double GstAmount = DeliveryAppWhiterocks.Models.Constants.taxAmount;
            double GstTotal = 0;

            List<LineItemSQLite> lineItems = App.LineItemDatabase.GetLineItemByInvoiceID(_selectedInvoice.InvoiceID);
            foreach (LineItemSQLite lineItemsSql in lineItems)
            {
                ItemSQLite itemSql = App.ItemDatabase.GetItemByID(lineItemsSql.ItemCode);
                LineItem item = new LineItem()
                {
                    Description = itemSql.Description,
                    Quantity = lineItemsSql.Quantity,
                    UnitAmount = lineItemsSql.UnitAmount,
                    TotalAmount = lineItemsSql.Quantity * lineItemsSql.UnitAmount
                };
                itemsSubtotal += item.TotalAmount;
                _itemsCollection.Add(item);
            }

            GstTotal = GstAmount * itemsSubtotal;
            LineItem GST = new LineItem() {
                Description = string.Format("GST: {0} %", GstAmount * 100),
                Quantity = 1,
                UnitAmount = GstTotal,
                TotalAmount = GstTotal,
            };
            _itemsCollection.Add(GST);

            LblTotalPrice.Text = string.Format("{0:F2}",GstTotal + itemsSubtotal);
            ItemsListView.ItemsSource = _itemsCollection;
        }

        private void TapClose_Tapped(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void TapLocation_Tapped(object sender, EventArgs e)
        {
            //LOAD MAP
        }

        private async void MarkAsCompleted(object sender, EventArgs e)
        {
            bool userAction = false;
            if(_selectedInvoice.Status == "Completed")
            {
                userAction = await DisplayAlert("Confirm action", "Do you wish to mark it as incomplete? ", "Yes", "Cancel");
                if (userAction)
                {
                    _selectedInvoice.Status = "";
                    CompletedOrderButton.Text = "Mark as Completed";
                    PageHeaderLabel.Text = _selectedInvoice.InvoiceNumber;
                    PageHeaderLabel.Text += _selectedInvoice.Type == "ACCPAY" ? " - Pickup" : " - Dropoff";
                }
            }
            else
            {
                userAction = await DisplayAlert("Confirm action", "Do you wish to mark it as completed? ", "Yes", "Cancel");
                if (userAction)
                {
                    _selectedInvoice.Status = "Completed";
                    CompletedOrderButton.Text = "Mark as incomplete";
                    PageHeaderLabel.Text = $"{_selectedInvoice.InvoiceNumber} Completed";
                    PageHeaderLabel.Text += _selectedInvoice.Type == "ACCPAY" ? " - Pickup" : " - Dropoff";
                }
            }
            
            if (userAction)
            {
                InvoiceSQLite invoice = App.InvoiceDatabase.GetInvoiceByInvoiceID(_selectedInvoice.InvoiceID);
                invoice.CompletedDeliveryStatus = (_selectedInvoice.Status == "Completed");
                invoice.UpdateTimeTicksApp = DateTime.Now.Ticks;
                
                App.InvoiceDatabase.UpdateInvoiceStatus(invoice);
            }
            
            // Navigation.PopModalAsync();

        } // Mark As Completed
    }
}