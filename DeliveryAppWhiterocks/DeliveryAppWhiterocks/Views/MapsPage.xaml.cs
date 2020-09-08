using DeliveryAppWhiterocks.Models;
using DeliveryAppWhiterocks.Models.Database.SQLite;
using DeliveryAppWhiterocks.Models.GoogleDirectionAPI;
using DeliveryAppWhiterocks.Models.Helper;
using DeliveryAppWhiterocks.Models.XeroAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.Xaml;

namespace DeliveryAppWhiterocks.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapsPage : ContentPage
    {
        List<string> _waypoints = new List<string>();

        //properties for sliding up menu
        
        PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        double _transY;
        //end sliding up menu props

        ObservableCollection<Invoice> _invoicesCollection = new ObservableCollection<Invoice>();
        List<Invoice> _invoices = new List<Invoice>();
        List<InvoiceSQLite> _invoiceSQLite = new List<InvoiceSQLite>();

        List<Pin> _pins = new List<Pin>();

        int _counter = 0;
        bool _firstVisited = true;

        Timer _timer;
        Location _currentLocation;
        Location _prevLocation;
        int _outsideRouteUpdateCounter = 0;

        GoogleDirection _direction;
       

        //DEST CONSTANT ONLY FOR TESTING REMOVE LATER
        int numberOfAPICalls = 0;
        static Position destination = new Position(-46.4134, 168.3556);

        //remove the passing parameter later. now is used only for testing
        public MapsPage(List<Invoice> invoices)
        {
            InitializeComponent();
            _invoices = invoices;
            
            //Enable the blue circle that mark the current location of user
            map.MyLocationEnabled = true;
            CenterMapToCurrentLocation();
            _timer = new Timer((e) =>
            {
                Device.BeginInvokeOnMainThread(async () => {
                    try {
                        if (App.CheckIfInternet()) { 
                            await Update();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Error");
                    }
                });
            }, null, 10, (int)TimeSpan.FromSeconds(6).TotalMilliseconds);

            //Got the information from https://winstongubantes.blogspot.com/2017/11/creating-draggable-sliding-up-panel-in.html
            InitMenu(); 
            InitializeObservables();
        }

        private async Task<bool> Update()
        {
            _currentLocation = await Geolocation.GetLastKnownLocationAsync();
            if (_currentLocation == null)
            {
                return false;
            }

            if (_prevLocation == null) _prevLocation = _currentLocation;


            if (map.Polylines.Count > 0 && _prevLocation != _currentLocation)
            {
                if (map.Polylines[0].Positions.Count == 0) return false;
                double kilometersDistanceToPointA = Location.CalculateDistance(_currentLocation, _prevLocation, DistanceUnits.Kilometers);
                //this is Point A, update the point A
                _prevLocation = _currentLocation;
                map.Polylines[0].Positions[0] = new Position(_currentLocation.Latitude, _currentLocation.Longitude);
                Position pointB = new Position(0, 0);
                if (map.Polylines[0].Positions.Count > 0)
                {
                    //this is Point B, next point
                    pointB = map.Polylines[0].Positions[1];
                }
                else if (map.Polylines[0].Positions.Count == 1)
                {
                    pointB = destination;
                }

                Location locationB = new Location(pointB.Latitude, pointB.Longitude);
                double kilometersDistanceToPointB = Location.CalculateDistance(_currentLocation, locationB, DistanceUnits.Kilometers);
                if (kilometersDistanceToPointA * 1000 > 10 && kilometersDistanceToPointB * 1000 < 35)
                {
                    _outsideRouteUpdateCounter = 0;
                    map.Polylines[0].Positions.RemoveAt(0);
                }
                else if(kilometersDistanceToPointA * 1000 > 10)
                {
                    _outsideRouteUpdateCounter++;
                }
            }

            if (_outsideRouteUpdateCounter >= 5)
            {
                _outsideRouteUpdateCounter = 0;
                MapDirections("(stray) ");
            }
            return true;
        }

        protected async override void OnAppearing()
        {
            _invoiceSQLite = App.InvoiceDatabase.GetAllIncompleteInvoices();
            if (_counter == 0) _counter = _invoiceSQLite.Count();
            if (_counter > _invoiceSQLite.Count() || _firstVisited)
            {
                await InitPins();
                MapDirections("(initializing) ");
                _counter = _invoiceSQLite.Count();
                _firstVisited = false;
                _outsideRouteUpdateCounter = 0;
            }
        }

        //SLIDEUP MENU REGION
        //a layout that helps in resizing the stacklayout that contains the delivery orders
        #region SlideUpMenu

        private void InitMenu()
        {
            Task.Factory.StartNew(async () =>
            {
                await Task.Delay(200);
                Device.BeginInvokeOnMainThread(() =>
                {
                    Notification.HeightRequest = this.Height - QuickMenuLayout.Height;
                    QuickMenuPullLayout.TranslationY = Notification.HeightRequest;
                });
            });

            QuickMenuPullLayout.BackgroundColor = new Color(247, 247, 247, 0.9);
        }

        private void InitializeObservables()
        {
            //IF THERE IS OBSERVABLES
            var panGestureObservable = Observable
                .FromEventPattern<PanUpdatedEventArgs>(
                    x => _panGesture.PanUpdated += x,
                    x => _panGesture.PanUpdated -= x
                )
                //.Throttle(TimeSpan.FromMilliseconds(20), TaskPoolScheduler.Default)
                .Subscribe(x => Device.BeginInvokeOnMainThread(() => { CheckQuickMenuPullOutGesture(x); }));

            QuickMenuInnerLayout.GestureRecognizers.Add(_panGesture);
        }

        private void CheckQuickMenuPullOutGesture(EventPattern<PanUpdatedEventArgs> x)
        {
            var e = x.EventArgs;
            var typeOfAction = x.Sender as StackLayout;

            switch (e.StatusType)
            {
                case GestureStatus.Running:
                    MethodLockedSync(() =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            //QuickMenuPullLayout.TranslationY = Math.Max(0,
                            //    Math.Min(Notification.HeightRequest, QuickMenuPullLayout.TranslationY + e.TotalY));
                            QuickMenuPullLayout.TranslateTo(QuickMenuPullLayout.TranslationX, Math.Max(60, Math.Min(Notification.HeightRequest, QuickMenuPullLayout.TranslationY + e.TotalY)), 250, Easing.Linear);
                        });
                    }, 2);

                    break;

                case GestureStatus.Completed:
                    // Store the translation applied during the pan
                    _transY = QuickMenuPullLayout.TranslationY;
                    break;
                case GestureStatus.Canceled:
                    
                    break;
            }
        }

        private CancellationTokenSource _throttleCts = new CancellationTokenSource();
        private void MethodLockedSync(Action method, double timeDelay = 500)
        {
            Interlocked.Exchange(ref _throttleCts, new CancellationTokenSource()).Cancel();
            Task.Delay(TimeSpan.FromMilliseconds(timeDelay), _throttleCts.Token) // throttle time
                .ContinueWith(
                    delegate { method(); },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        #endregion

        private async Task<bool> InitPins()
        {
            _pins.Clear();
            map.Pins.Clear();
            _waypoints.Clear();

            foreach (InvoiceSQLite invoice in _invoiceSQLite)
            {
                ContactSQLite customerContact = App.ContactDatabase.GetContactByID(invoice.ContactID);
                Position position;

                if (!customerContact.Latitude.HasValue) { 

                    //Get better format from address
                    #region Format the Address
                    string fullAddress = customerContact.Address;
                    if(fullAddress == "")
                    {
                        return true;
                    }
                    else if (customerContact.City != "")
                    {
                        fullAddress += $", {customerContact.City}";
                    }
                    fullAddress += $", New Zealand";
                    #endregion

                    if (App.CheckIfInternet()) {
                        //Get location by calling google geolocation API / each invoice
                        position = await GoogleMapsAPI.GetPositionFromKnownAddress(fullAddress);
                        customerContact.Latitude = position.Latitude;
                        customerContact.Longitude = position.Longitude;
                        
                    } else
                    {
                        try { 
                            Location location = Geocoding.GetLocationsAsync(fullAddress).Result.FirstOrDefault();
                            position = new Position(location.Latitude, location.Longitude);
                            customerContact.Latitude = position.Latitude;
                            customerContact.Longitude = position.Longitude;
                        }
                        catch
                        {
                            position = new Position(0, 0);
                            await DisplayAlert("Alert", String.Format("Couldnt map the position of {0}",invoice.InvoiceNumber), "OK");
                        }
                    }

                    if(customerContact.Latitude.HasValue) App.ContactDatabase.UpdateContactPosition(customerContact);
                } else 
                {
                    position = new Position(customerContact.Latitude.Value, customerContact.Longitude.Value);
                }

                if(position.Latitude != 0 && position.Longitude != 0) { 
                    //Add it to the waypoints list later to be used on the googleDirection API
                    //Formatted by Comma separator for latitude,longitude
                    _waypoints.Add($"{position.Latitude}%2C{position.Longitude}");

                    //Set pin on map
                    #region SetPin
                    var pin = new Pin()
                    {
                        Position = position,
                        Label = $"{invoice.InvoiceNumber}",
                        //set tag so i can reference it when a pin is clicked
                        Tag = invoice,
                        Icon = BitmapDescriptorFactory.FromBundle(invoice.InvoiceType),
                    };
                    
                    _pins.Add(pin);

                    map.SelectedPinChanged += Map_SelectedPinChanged;
                    map.Pins.Add(pin);
                    #endregion
                }
            }
            return true;
        }

        //InitPins() should be called before this method, _waypoints is added in InitPins()
        //A method that let's google handle the directions, shortest path etc.
        private async void MapDirections(string message="")
        {
            //for diagnostic
            numberOfAPICalls++;
            Console.WriteLine($"{message} Number Of API Calls: #{numberOfAPICalls}");

            map.Polylines.Clear();
            if (_currentLocation == null)
            {
                _currentLocation = await Geolocation.GetLastKnownLocationAsync();
            }
            Position lastKnownPosition = new Position(_currentLocation.Latitude, _currentLocation.Longitude);

            if (_waypoints.Count > 0 && App.CheckIfInternet()) {
                
                _invoicesCollection.Clear();

                _direction = await GoogleMapsAPI.MapDirectionsWithWaypoints(lastKnownPosition, _waypoints.ToArray());

                //Only create a line if it returns something from google
                if(_direction.Status != "ZERO_RESULTS" && _direction.Routes.Count > 0) { 
                    List<Position> directionPolylines = PolylineHelper.Decode(_direction.Routes[0].OverviewPolyline.Points).ToList();

                    CreatePolylinesOnMap(directionPolylines);

                    foreach (int order in GoogleMapsAPI._waypointsOrder)
                    {
                        _invoicesCollection.Add(_invoices[order]);
                    }

                    DeliveryItemView.ItemsSource = _invoicesCollection;
                } else
                {
                    await DisplayAlert("Oops","Unable to map the directions, please try to use internet connections or restart the app","OK");
                }
            } else if(_waypoints.Count() == 0 && App.CheckIfInternet())
            {
                _direction = await GoogleMapsAPI.MapDirectionsNoWaypoints(lastKnownPosition);
                if (_direction.Status != "ZERO_RESULTS" && _direction.Routes.Count > 0)
                {
                    List<Position> directionPolylines = PolylineHelper.Decode(_direction.Routes[0].OverviewPolyline.Points).ToList();
                    CreatePolylinesOnMap(directionPolylines);
                }
            }
        }

        private void CreatePolylinesOnMap(List<Position> directionPolylines)
        {
            Xamarin.Forms.GoogleMaps.Polyline polyline = new Xamarin.Forms.GoogleMaps.Polyline()
            {
                StrokeColor = Constants.mapShapeColor,
                StrokeWidth = 8,
            };

            for (int i = 0; i < directionPolylines.Count ; i++)
            {
                polyline.Positions.Add(directionPolylines[i]);
            }
            map.Polylines.Add(polyline);
        }

        //Do something when a pin is clicked, experimental
        private void Map_SelectedPinChanged(object sender, SelectedPinChangedEventArgs e)
        {
            Pin currentPinSelected = e.SelectedPin;
        }

        //A method that is centering map to the location of the device
        private async void CenterMapToCurrentLocation()
        {
            if(_currentLocation == null)
            {
                _currentLocation = await Geolocation.GetLastKnownLocationAsync();
            }
            
            Position lastKnownPosition = new Position(_currentLocation.Latitude, _currentLocation.Longitude);
            map.MoveToRegion(new MapSpan(lastKnownPosition, 0.05, 0.05));
        }

        //tied to the XAML side, x:Name=currentLocationButton
        private void currentLocationButton_Clicked(object sender, EventArgs e)
        {
            CenterMapToCurrentLocation();
        }
        
        //When the see items button is clicked it will go to the order details page
        private async void ItemDetailsButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            Invoice invoiceSelected = button.BindingContext as Invoice;
            
            await Navigation.PushModalAsync(new OrderDetailPage(invoiceSelected));
        }

        private async void MarkAsCompleted(object sender, EventArgs e)
        {
            bool userAction = await DisplayAlert("Confirm action", "Do you wish to mark it as complete? ", "Yes", "Cancel");

            if (userAction)
            {
                var button = sender as Button;
               
                Invoice invoiceSelected = button.BindingContext as Invoice;
                invoiceSelected.Status = "Completed";

                InvoiceSQLite invoice = new InvoiceSQLite();
                invoice.InvoiceID = invoiceSelected.InvoiceID;
                invoice.InvoiceNumber = invoiceSelected.InvoiceNumber;
                invoice.CompletedDeliveryStatus = (invoiceSelected.Status == "Completed");
                invoice.ContactID = invoiceSelected.Contact.ContactID;
                invoice.Subtotal = invoiceSelected.SubTotal;
                invoice.TenantID = Preferences.Get("TenantID", string.Empty);

                App.InvoiceDatabase.UpdateInvoiceStatus(invoice);
                UpdateStatus(invoiceSelected);
            }
        } // Mark As Completed

        public void UpdateStatus(Invoice invoiceSelected)
        {
            bool googleAPICallRequired = true;

            _invoicesCollection.Remove(invoiceSelected);
            _invoices.Remove(invoiceSelected);
            DeliveryItemView.ItemsSource = _invoicesCollection;
            Pin thePin = _pins.Where(pinX => pinX.Label == invoiceSelected.InvoiceNumber).FirstOrDefault();
            map.Pins.Remove(thePin);
            _waypoints.Remove($"{thePin.Position.Latitude}%2C{thePin.Position.Longitude}");

            InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceNumber(invoiceSelected.InvoiceNumber);
            if(invoiceSQLite != null)
            {
                ContactSQLite contact =  App.ContactDatabase.GetContactByID(invoiceSQLite.ContactID);
                double distanceToInvoiceInKm = Location.CalculateDistance(_currentLocation, new Location((double)contact.Latitude,(double)contact.Longitude), DistanceUnits.Kilometers);

                if (distanceToInvoiceInKm * 1000 < 30) googleAPICallRequired = false;
            }

            if(googleAPICallRequired) MapDirections("(Force-Refresh) ");
            _counter--;
        }
    }
}