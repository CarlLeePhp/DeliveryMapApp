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
        public CompletedPage()
        {
            InitializeComponent();
            BindingContext = new CompletedViewModel();
        }

        private void DeliveryInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}