using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using DeliveryAppWhiterocks.Models;
using Xamarin.Essentials;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using Xamarin.Forms.GoogleMaps;
using DeliveryAppWhiterocks.Models.GoogleDirectionAPI;

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
                Validate(() => _taxAmount > 0 && _taxAmount < 1, "GST should between 0 and 1");
                if(_taxAmount > 0 && _taxAmount < 1) Constants.taxAmount = TaxAmount;
                OnPropertyChanged();
            }
        }
        private IList<TenantSQLite> _tenants;
        public IList<TenantSQLite> Tenants
        {
            get { return _tenants; }
            set
            {
                _tenants = value;
                OnPropertyChanged();

            }
        }
        private TenantSQLite _selectedTenant;

        public TenantSQLite SelectedTenant
        {
            get { return _selectedTenant; }
            set {
                _selectedTenant = value;
                OnPropertyChanged();
                Preferences.Set("TenantID", _selectedTenant.TenantID);
            }
        }

        private TenantSQLite _currentTenant;

        public TenantSQLite CurrentTenant
        {
            get { return _currentTenant; }
            set { 
                _currentTenant = value; 
            }
        }



        public Command CloseCommand { get; set; }
        public Command EndPointCompleted { get; set; }
        public SettingViewModel(INavigation navigation)
        {
            _navigation = navigation;
            TaxAmount = Constants.taxAmount;
            EndPoint = Preferences.Get("EndPoint", "");
            Tenants = App.TenantDatabase.GetAllTenants();
            // Register Commands
            CloseCommand = new Command(CloseView);
            EndPointCompleted = new Command(CheckEndPoint);
        }

        private async void CloseView()
        {
            await _navigation.PopModalAsync();
        }
        private async void CheckEndPoint()
        {
            if(EndPoint == Preferences.Get("EndPoint", ""))
            {
                return;
            }
            Position position = await GoogleMapsAPI.GetPositionFromKnownAddress(_endPoint);
            bool isSuccess = !(position.Latitude == 0 && position.Longitude == 0);
            if (isSuccess)
            {
                Preferences.Set("EndPoint", EndPoint);
                Preferences.Set("EndPointGeoWaypoint", $"{position.Latitude}%2C{position.Longitude}");
                GoogleMapsAPI.DestinationAddress = EndPoint;
                GoogleMapsAPI.DestinationPosition = position;
            }
            
            Validate(() => !isSuccess, "Set the End Point as " + _endPoint, "Message");
            Validate(() => isSuccess, "The address is not valid, Please try again", "Message"); // Message is not a real property name
        }
    }
}
