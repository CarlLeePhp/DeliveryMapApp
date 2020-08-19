using DeliveryAppWhiterocks.Models;
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
    public partial class DeliveryInfoPage : ContentPage
    {
        ObservableCollection<Stock> _stockInfo = new ObservableCollection<Stock>();
       
        public DeliveryInfoPage()
        {
            InitializeComponent();
            Init();
            MapItemList();
        }


        private void Init()
        {
            App.CheckInternetIfConnected(noInternetLbl, this);

            if (Device.Idiom == TargetIdiom.Tablet)
            {
                double tabletFontSize = Device.GetNamedSize(NamedSize.Title, typeof(Label));
                QuantityHeaderLabel.FontSize = tabletFontSize;
                ItemDescHeaderLabel.FontSize = tabletFontSize;
            }
            else
            {
                double phoneFontSize = Device.GetNamedSize(NamedSize.Subtitle, typeof(Label));
                QuantityHeaderLabel.FontSize = phoneFontSize;
                ItemDescHeaderLabel.FontSize = phoneFontSize;
            }
        }

        private void MapItemList()
        {
            Dictionary<string, Stock> itemDictionary = new Dictionary<string, Stock>();

            List<InvoiceSQLite> invoiceSQLite = App.InvoiceDatabase.GetAllIncompleteInvoices();

            foreach (InvoiceSQLite invoice in invoiceSQLite)
            {
                List<LineItemSQLite> lineItemSQLite = App.LineItemDatabase.GetLineItemByInvoiceID(invoice.InvoiceID);
                foreach(LineItemSQLite lineItem in lineItemSQLite)
                {
                    string codeX = lineItem.ItemCode;
                    ItemSQLite itemSQLite = App.ItemDatabase.GetItemByID(lineItem.ItemCode);
                    if (!itemDictionary.ContainsKey(codeX))
                    {
                        Stock stock = new Stock(codeX, itemSQLite.Description, itemSQLite.Weight, lineItem.Quantity);
                        itemDictionary.Add(codeX, stock);
                    }
                    else
                    {
                        itemDictionary[codeX].AddStockQuantity(Convert.ToInt32(lineItem.Quantity));
                        itemDictionary[codeX].AddStockWeight(itemSQLite.Weight);
                    }
                }
            }

            double totalWeight = 0;
            foreach (KeyValuePair<string, Stock> stock in itemDictionary)
            {
                Stock stockX = stock.Value;
                stockX.SetColor();
                _stockInfo.Add(stockX);
                totalWeight += stockX.Weight;
            }
            WeightTotalLabel.Text = string.Format($"{totalWeight:F2} Kg");
            DeliveryItemListView.ItemsSource = _stockInfo;
        }

        private async void TapBack_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}