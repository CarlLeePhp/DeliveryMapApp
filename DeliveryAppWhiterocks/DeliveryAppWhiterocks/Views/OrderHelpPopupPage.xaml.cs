using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliveryAppWhiterocks.Models;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace DeliveryAppWhiterocks.Views
{
   
    public partial class OrderHelpPopupPage :PopupPage
    {
        public OrderHelpPopupPage()
        {
            InitializeComponent();
            if(Device.Idiom == TargetIdiom.Phone) { 
                popupContainerLayout.WidthRequest = App.screenWidth - 40;
            }
            
        }

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}