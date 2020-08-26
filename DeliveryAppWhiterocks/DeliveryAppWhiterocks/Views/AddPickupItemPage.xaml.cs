using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using dotMorten.Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddPickupItemPage : ContentPage
    {
        List<string> itemsNameList = new List<string>();
        List<ItemSQLite> itemsSQLite = new List<ItemSQLite>();
        AddPickupPage pickupPage;
        
        public AddPickupItemPage(AddPickupPage page)
        {
            InitializeComponent();
            pickupPage = page;
            itemsSQLite = pickupPage.itemsSQLite;
            Init();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            App.CheckInternetIfConnected(noInternetLbl, this);
            foreach (ItemSQLite item in itemsSQLite)
            {
                itemsNameList.Add(item.Description);
            }
        }

        private void TapBack_Tapped(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ItemAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, dotMorten.Xamarin.Forms.AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            if (sender.Text != "")
            {
                int index = itemsNameList.IndexOf(sender.Text);
                if (index == -1) return;
                sender.Text = itemsSQLite[index].Description;
                WeightEntry.Text = itemsSQLite[index].Weight.ToString();
                UnitPriceEntry.Text = itemsSQLite[index].UnitCost.ToString();
            }
        }

        private void ItemAutoSuggestBox_TextChanged(AutoSuggestBox sender, dotMorten.Xamarin.Forms.AutoSuggestBoxTextChangedEventArgs e)
        {
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text != "")
            {
                sender.ItemsSource = itemsNameList.Where(i => i.StartsWith(ItemAutoSuggestBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
        }

        private void Quantity_TextChanged(Entry sender, TextChangedEventArgs e)
        {
            if(!sender.Text.Any(char.IsDigit))
            {
                sender.Text = "";
            }
        }

        private void UnitPrice_TextChanged(Entry sender, TextChangedEventArgs e)
        {
            double amount;
            if(sender.Text == "0")
            {
                sender.Text = "";
            }
            else if(!double.TryParse(sender.Text, out amount))
            {
                sender.Text = "";
            }
        }

        private void Weight_TextChanged(Entry sender, TextChangedEventArgs e)
        {
            double weight;
            if (sender.Text == "0")
            {
                sender.Text = "";
            }
            else if (!double.TryParse(sender.Text, out weight))
            {
                sender.Text = "";
            }
        }

        private async void AddItemButton_Clicked(object sender, EventArgs e)
        {
            if(ItemAutoSuggestBox.Text == "")
            {
                await DisplayAlert("Alert", "Please Enter Item Description", "OK");
            }
            else if(QuantityEntry.Text == "" || WeightEntry.Text == "" || UnitPriceEntry.Text == "" )
            {
                if (await DisplayAlert("Warning", "Not All Entry Field is filled, Continue?", "Yes", "Cancel"))
                {
                    SaveItem();
                } else
                {
                    return;
                }
            } else
            {
                SaveItem();
            }

        }

        private void SaveItem()
        {
            int quantityF;
            int.TryParse(QuantityEntry.Text, out quantityF);

            double weightF;
            double.TryParse(WeightEntry.Text, out weightF);

            double unitCostF;
            double.TryParse(UnitPriceEntry.Text, out unitCostF);

            ItemSQLite item = itemsSQLite.Where(
                itemX => 
                itemX.Weight == weightF &&
                itemX.Description == ItemAutoSuggestBox.Text).FirstOrDefault();

            if(item == null)
            {
                item = new ItemSQLite()
                {
                    ItemCode = DateTime.Now.ToString(),
                    Description = ItemAutoSuggestBox.Text,
                    Weight = weightF,
                    UnitCost = unitCostF,
                };
            }
            else if (item.UnitCost == 0 || item.UnitCost != unitCostF)
            {
                item.UnitCost = unitCostF;
            }

            LineItem lineItem = new LineItem()
            {
                ItemCode = item.ItemCode,
                Description = item.Description,
                Quantity = quantityF,
                Weight = item.Weight,
                UnitAmount = unitCostF
            };
            pickupPage._itemsCollection.Add(lineItem);

            Navigation.PopModalAsync();
        }
    }
}