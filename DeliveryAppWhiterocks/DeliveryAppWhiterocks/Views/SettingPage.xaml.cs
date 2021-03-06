﻿using DeliveryAppWhiterocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using DeliveryAppWhiterocks.ViewModels;
using System.ComponentModel;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using System.Xml.XPath;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        private INavigation _navigation;
        SettingViewModel ViewModel => BindingContext as SettingViewModel;
        public SettingPage(INavigation navigation)
        {
            InitializeComponent();
            _navigation = navigation;
            App.CheckInternetIfConnected(noInternetLbl, this);
            BindingContextChanged += Page_BindingContextChanged;
            BindingContext = new SettingViewModel(_navigation);
        }


        void Page_BindingContextChanged(object sender, EventArgs e)
        {
            ViewModel.ErrorsChanged += ViewModel_ErrorChanged;
        }
        async void ViewModel_ErrorChanged(object sender, DataErrorsChangedEventArgs e)
        {
            var propHasErrors = (ViewModel.GetErrors(e.PropertyName) as List<string>)?.Any() == true;
            if (propHasErrors)
            {
                string result = "";
                foreach (string msg in ViewModel.GetErrors(e.PropertyName) as List<string>){
                    result += msg;
                    result += "\n";
                }
                await DisplayAlert("Message", result, "OK");
            }
            
            //switch (e.PropertyName)
            //{
            //    case nameof(ViewModel.EndPoint):
            //        endPoint.LabelColor = propHasErrors ? Color.Red : Color.Gray;
            //        break;
            //    case nameof(ViewModel.TaxAmount):
            //        taxAmount.LabelColor = propHasErrors ? Color.Red : Color.Gray;
            //        break;
            //    default:
            //        break;
            //}
        }
        private async void TapClose_Tapped(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        
    }
}