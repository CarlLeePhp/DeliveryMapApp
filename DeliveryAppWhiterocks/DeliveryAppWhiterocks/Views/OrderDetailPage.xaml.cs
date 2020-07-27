using DeliveryAppWhiterocks.Models.Database.SQLite;
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

            List<LineItemSQLite> lineItems = App.LineItemDatabase.GetLineItemByInvoiceID(_selectedInvoice.InvoiceID);
            foreach (LineItemSQLite lineItemsSql in lineItems)
            {
                ItemSQLite itemSql = App.ItemDatabase.GetItemByID(lineItemsSql.ItemCode);
                LineItem item = new LineItem()
                {
                    Description = itemSql.Description,
                    Quantity = lineItemsSql.Quantity,
                    UnitAmount = itemSql.UnitAmount,
                    TotalAmount = lineItemsSql.Quantity * itemSql.UnitAmount
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
    }
}