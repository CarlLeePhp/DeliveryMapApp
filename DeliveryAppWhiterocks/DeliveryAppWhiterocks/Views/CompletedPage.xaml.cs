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
        public CompletedPage()
        {
            InitializeComponent();
            
        }

        protected override void OnAppearing()
        {
            _childPageLoaded = false;
            init();
            BindingContext = new CompletedViewModel(Navigation);
        }
        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSelection = e.CurrentSelection.FirstOrDefault() as Invoice;
            if (currentSelection == null || _childPageLoaded) return;
            _childPageLoaded = true;
            Navigation.PushModalAsync(new OrderDetailPage(currentSelection));

            ((CollectionView)sender).SelectedItem = null;
        }

        private void init()
        {
            App.CheckInternetIfConnected(noInternetLbl, this);
        }
    }
}