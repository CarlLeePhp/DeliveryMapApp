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
    public partial class ViewCardsIndicator : ContentView
    {
        public ViewCardsIndicator()
        {
            InitializeComponent();
            DropOffIndicator.BackgroundColor = Constants.IsDropOffColor;
            PickupIndicator.BackgroundColor = Constants.IsPickUpColor;
        }
    }
}