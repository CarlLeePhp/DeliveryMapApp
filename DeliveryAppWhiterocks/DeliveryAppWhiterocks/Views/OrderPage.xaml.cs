using DeliveryAppWhiterocks.Models;
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
    public partial class OrderPage : ContentPage
    {
        public OrderPage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            NavigationPage.SetHasNavigationBar(this, false);
            BackgroundColor = Constants.contrastColor;

            
        }
    }
}