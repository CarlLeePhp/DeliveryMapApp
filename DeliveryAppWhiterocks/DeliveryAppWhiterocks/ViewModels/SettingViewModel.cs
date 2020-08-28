using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using Xamarin.Essentials;

namespace DeliveryAppWhiterocks.ViewModels
{
    class SettingViewModel : BaseViewModel
    {
        INavigation _navigation;
        private string _endPoint;

        public string EndPoint
        {
            get { return _endPoint; }
            set 
            {
                _endPoint = value;
                OnPropertyChanged();
            }
        }

        private double _taxAmount;

        public double TaxAmount
        {
            get { return _taxAmount; }
            set
            {
                _taxAmount = value;
                OnPropertyChanged();
            }
        }

        public Command SaveCommand { get; set; }
        public Command CloseCommand { get; set; }

        public SettingViewModel(INavigation navigation)
        {
            _navigation = navigation;
            TaxAmount = Constants.taxAmount;
            EndPoint = Preferences.Get("EndPoint", "");
            // Register Commands
            SaveCommand = new Command(Save);
            CloseCommand = new Command(CloseView);

        }

        private async void Save()
        {
            Constants.taxAmount = TaxAmount;
            Preferences.Set("EndPoint", EndPoint);
            await _navigation.PopModalAsync();
        }
        private async void CloseView()
        {
            await _navigation.PopModalAsync();
        }
    }
}
