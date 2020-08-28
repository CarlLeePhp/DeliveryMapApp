using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using Xamarin.Essentials;

namespace DeliveryAppWhiterocks.ViewModels
{
    class SettingViewModel : BaseValidationViewModel
    {
        INavigation _navigation;
        private string _endPoint;

        public string EndPoint
        {
            get { return _endPoint; }
            set 
            {
                _endPoint = value;
                Validate(() => !string.IsNullOrWhiteSpace(_endPoint), "End Point must be provided.");
                OnPropertyChanged();
                SaveCommand.ChangeCanExecute();
            }
        }

        private double _taxAmount;

        public double TaxAmount
        {
            get { return _taxAmount; }
            set
            {
                _taxAmount = value;
                Validate(() => _taxAmount > 0 && _taxAmount < 1, "GST must be between 0 and 1");
                OnPropertyChanged();
                SaveCommand.ChangeCanExecute();
            }
        }

        Command _saveCommand;
        public Command SaveCommand => _saveCommand ?? (_saveCommand = new Command(Save, CanSave));
        private async void Save()
        {
            Constants.taxAmount = TaxAmount;
            Preferences.Set("EndPoint", EndPoint);
            await _navigation.PopModalAsync();
        }
        bool CanSave() => !string.IsNullOrWhiteSpace(EndPoint) && !HasErrors;
        public Command CloseCommand { get; set; }

        public SettingViewModel(INavigation navigation)
        {
            _navigation = navigation;
            TaxAmount = Constants.taxAmount;
            EndPoint = Preferences.Get("EndPoint", "");
            // Register Commands
            CloseCommand = new Command(CloseView);

        }

        
        private async void CloseView()
        {
            await _navigation.PopModalAsync();
        }
    }
}
