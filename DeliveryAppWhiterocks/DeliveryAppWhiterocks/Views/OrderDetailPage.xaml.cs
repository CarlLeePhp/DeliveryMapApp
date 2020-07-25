using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            PageHeaderLabel.Text = _selectedInvoice.InvoiceNumber;
            customerNameLabel.Text = _selectedInvoice.Contact.Name;
            customerAddressLabel.Text = $"{_selectedInvoice.Contact.Addresses[1].AddressLine1}, {_selectedInvoice.Contact.Addresses[1].City}";

            App.CheckInternetIfConnected(noInternetLbl, this);
        }

        private void BindItemData()
        {
            double itemsSubtotal = 0;
            double GstAmount = DeliveryAppWhiterocks.Models.Constants.taxAmount;
            double GstTotal = 0;
            foreach (LineItem item in _selectedInvoice.LineItems)
            {
                item.TotalAmount = item.Quantity * item.UnitAmount;
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
    }
}