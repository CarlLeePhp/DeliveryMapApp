using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.ViewModels
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ExpandableLabel : ContentView
    {
        private bool isExpanding;
        private bool isExpanded = false;

        public ExpandableLabel()
        {
            InitializeComponent();
            ExpandableLayout.HeightRequest = 0;
            Init();
        }

        private void Init()
        {

            Wrapper.BackgroundColor = Constants.mainTextColor;
            
            Header.BackgroundColor = Constants.backgroundColor;
            TitleText.TextColor = Constants.mainTextColor;
            ExpandableLayout.BackgroundColor = Constants.backgroundColor;
            ExpandableContent.BackgroundColor = Constants.backgroundColor;
            ExpandableText.TextColor = Constants.mainTextColor;
            

        }

        private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
             
                if (!isExpanded) { 
                    ExpandableLayout.HeightRequest = 300;
                } else
                {
                    ExpandableLayout.HeightRequest = 50;
                }

            isExpanded = !isExpanded;
        }
    }
}