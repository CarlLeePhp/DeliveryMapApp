using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.XeroAPI;
using DeliveryAppWhiterocks.ViewModels;
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
    public partial class CompletedPage : ContentPage
    {
        bool _childPageLoaded = false;
        Invoice _currentSelected;

        public CompletedPage()
        {
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            _childPageLoaded = false;
            init();
            if(_currentSelected == null) { 
                BindingContext = new CompletedViewModel(Navigation);
            } else
            {
                CompletedViewModel model = BindingContext as CompletedViewModel;
                int selectedIndex = model.DeliveryOrders.IndexOf(_currentSelected);

                InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceID(_currentSelected.InvoiceID);

                if (!invoiceSQLite.CompletedDeliveryStatus) { 
                    model.DeliveryOrders.Remove(_currentSelected);
                }
            }
        }
        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelection = e.CurrentSelection.FirstOrDefault() as Invoice;
            if (currentSelection == null || _childPageLoaded) return;
            _childPageLoaded = true;
            Navigation.PushModalAsync(new OrderDetailPage(currentSelection));
            _currentSelected = currentSelection;
            ((CollectionView)sender).SelectedItem = null;
        }

        private void init()
        {
            App.CheckInternetIfConnected(noInternetLbl, this);
        }
    }
}