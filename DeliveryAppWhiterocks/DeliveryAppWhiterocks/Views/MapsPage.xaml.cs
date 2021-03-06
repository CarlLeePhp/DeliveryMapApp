﻿using DeliveryAppWhiterocks.Models;
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
using System.Text.RegularExpressions;
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
        List<Invoice> _storageInvoice = new List<Invoice>();
        List<Invoice> _invoices = new List<Invoice>();
        List<InvoiceSQLite> _invoiceSQLite = new List<InvoiceSQLite>();
        Invoice _currentSelectedInvoice;

        List<Pin> _pins = new List<Pin>();

        int _counter = 0;
        bool _firstVisited = true;
        bool _isDetailedPageOpen = false;

        Timer _timer;
        Location _currentLocation;
        Location _prevLocation;
        Location _savedLocation;
        int _outsideRouteUpdateCounter = 0;

        GoogleDirection _direction;
        //List<Step> _steps = new List<Step>();
        List<Leg> _legs = new List<Leg>();
        int _currentLegs = 0;

        double _currentWeight = 0;

        const int MAX_WAYPOINTS = 23; 

        //DEST CONSTANT ONLY FOR TESTING REMOVE LATER
        int numberOfAPICalls = 0;

        INavigation _navigation;

        //remove the passing parameter later. now is used only for testing
        public MapsPage(List<Invoice> invoices, INavigation navigation)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _storageInvoice = invoices;
            _navigation = navigation;
            //Enable the blue circle that mark the current location of user
            map.MyLocationEnabled = true;
            CenterMapToCurrentLocation();
            _timer = new Timer((e) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        //if (App.CheckIfInternet())
                        //{
                        await Update();
                    //}
                    }
                        catch
                    {
                        Console.WriteLine("Error");
                    }
                });
            }, null, 10, (int)TimeSpan.FromSeconds(5).TotalMilliseconds);

            //Got the information from https://winstongubantes.blogspot.com/2017/11/creating-draggable-sliding-up-panel-in.html
            InitMenu(); 
            InitializeObservables();
            List<InvoiceSQLite> tempInvoices = App.InvoiceDatabase.GetAllIncompletePickupInvoice();
            InitWeightLabel(tempInvoices);
        }

        private async Task<bool> Update()
        {
            _currentLocation = await Geolocation.GetLastKnownLocationAsync();
            if (_currentLocation == null )
            {
                return false;
            }

            if (_prevLocation == null) _prevLocation = _currentLocation;
            
            
            if (map.Polylines.Count > 0 )
            {
                if (map.Polylines[0].Positions.Count == 0) return false;
                
                map.Polylines[0].Positions[0] = new Position(_currentLocation.Latitude, _currentLocation.Longitude);
                Position pointB = new Position(0, 0);
                if (map.Polylines[0].Positions.Count > 0)
                {
                    //this is Point B, next point
                    pointB = map.Polylines[0].Positions[1];
                }

                Location locationB = new Location(pointB.Latitude, pointB.Longitude);

                double kilometersDistanceOld = Location.CalculateDistance(_prevLocation, locationB, DistanceUnits.Kilometers);
                double kilometersDistanceNew = Location.CalculateDistance(_currentLocation, locationB, DistanceUnits.Kilometers);
                //double kilometersDistanceOldVsNew = Location.CalculateDistance(_prevLocation, _currentLocation, DistanceUnits.Kilometers);

                if(kilometersDistanceNew < 0.07)
                {
                    _outsideRouteUpdateCounter = 0;
                    map.Polylines[0].Positions.RemoveAt(0);
                } else if (kilometersDistanceNew > kilometersDistanceOld || kilometersDistanceNew > 0.1)
                {
                    _outsideRouteUpdateCounter++;
                }

                //Text To speech
                if(_legs.Count > 0  && _currentLegs < _legs.Count ) {

                    if (_legs[_currentLegs].Steps.Count == 0 && Location.CalculateDistance(_currentLocation, new Location(_legs[_currentLegs].EndLocation.Lat, _legs[_currentLegs].EndLocation.Lng), DistanceUnits.Kilometers) < 0.35)
                    {
                        TextToSpeech.SpeakAsync("You have arrived at your destination");

                        _legs.RemoveAt(_currentLegs);
                    }
                    else if (_legs[_currentLegs].Steps.Count > 0) { 
                        double kilometersDistanceToStep = Location.CalculateDistance(_currentLocation, new Location(_legs[_currentLegs].Steps[0].StartLocation.Lat, _legs[_currentLegs].Steps[0].StartLocation.Lng), DistanceUnits.Kilometers);

                    
                        if (kilometersDistanceToStep < 0.045 )
                        {
                            string instruction = StripHTML(_legs[_currentLegs].Steps[0].HtmlInstructions).ToLower();
                            instruction = Regex.Replace(instruction, "( st)($| |,)", " street");
                            instruction = Regex.Replace(instruction, "( dr)($| |,)", " drive");
                            instruction = Regex.Replace(instruction, "( rd)($| |,|.)", " road");
                            instruction = Regex.Replace(instruction, "( ave)($| |,)", " avenue");
                            instruction = Regex.Replace(instruction, "( hwy)($| |,)", " highway");

                            TextToSpeech.SpeakAsync(instruction);

                            _legs[_currentLegs].Steps.RemoveAt(0);
                        }
                    }
                   
                }

                _prevLocation = _currentLocation;
            }

            if (_outsideRouteUpdateCounter >= 5)
            {
                Console.WriteLine("REFRESH");
                _outsideRouteUpdateCounter = 0;
                MapDirections("(stray) ");
            }
            return true;
        }

        public static string StripHTML(string input)
        {
            return Regex.Replace(input, "<.*?>", "");
        }

        protected async override void OnAppearing()
        {
            _isDetailedPageOpen = false;
            if (_currentSelectedInvoice != null)
            {
                InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceID(_currentSelectedInvoice.InvoiceID);
                if (invoiceSQLite.CompletedDeliveryStatus)
                {
                    UpdateStatus(_currentSelectedInvoice);
                    UpdateWeightLabel(_currentSelectedInvoice);
                    if (IsGoogleAPICallRequired(_currentSelectedInvoice)) MapDirections("(Force-Refresh) ");
                }
                _currentSelectedInvoice = null;
            }
            else
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
        }

        private void InitWeightLabel(List<InvoiceSQLite> invoiceSQLite)
        {
            Dictionary<string, Stock> itemDictionary = StockManager.GetStock(invoiceSQLite);

            _currentWeight = 0;
            foreach (KeyValuePair<string, Stock> stock in itemDictionary)
            {
                Stock stockX = stock.Value;
                _currentWeight += stockX.Weight;
            }
            
            TotalWeightLabel.Text = string.Format($"{_currentWeight:F2} Kg");
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
                    Notification.HeightRequest = MapContentLayout.Height - QuickMenuLayout.Height;
                    QuickMenuPullLayout.TranslationY = Notification.HeightRequest;
                });
            });

            MenuPaddingBottomStackLayout.HeightRequest = App.screenHeight / 2;
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
                            QuickMenuPullLayout.TranslateTo(QuickMenuPullLayout.TranslationX, Math.Max(0, Math.Min(Notification.HeightRequest, QuickMenuPullLayout.TranslationY + e.TotalY)), 250, Easing.Linear);
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
        
        //initialize markers on map and position into _waypoints
        private async Task<bool> InitPins()
        {
            _pins.Clear();
            map.Pins.Clear();
            _waypoints.Clear();

            for (int i=0; i <_invoiceSQLite.Count; i++)
            {
                ContactSQLite customerContact = App.ContactDatabase.GetContactByID(_invoiceSQLite[i].ContactID);
                Position position;

                if (!customerContact.Latitude.HasValue) { 

                    //Get better format from address
                    #region Format the Address
                    string fullAddress = customerContact.Address;
                    if(fullAddress == "")
                    {
                        continue;
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
                            await DisplayAlert("Alert", String.Format("Couldnt map the position of {0}", _invoiceSQLite[i].InvoiceNumber), "OK");
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
                        Label = $"{_invoiceSQLite[i].InvoiceNumber}",
                        //set tag so i can reference it when a pin is clicked
                        Tag = _invoiceSQLite[i].InvoiceID,
                        Icon = BitmapDescriptorFactory.FromBundle(_invoiceSQLite[i].InvoiceType),
                    };
                    pin.Address = "Click for Details";
                    _pins.Add(pin);

                    map.SelectedPinChanged += Map_SelectedPinChanged;
                    map.InfoWindowClicked += Map_InfoWindowClicked;
                    map.Pins.Add(pin);
                    #endregion

                    _invoices.Add(_storageInvoice[i]);
                }
            }
            return true;
        }

        //method to open invoice details by clicking label on pin
        private async void Map_InfoWindowClicked(object sender, InfoWindowClickedEventArgs e)
        {
            Pin clickedPin = e.Pin;
            string clickedInvoiceID = clickedPin.Tag as string;
            Invoice invoice = _invoicesCollection.Where(invoiceX => invoiceX.InvoiceID == clickedInvoiceID).FirstOrDefault();
            _currentSelectedInvoice = invoice;
            if (_isDetailedPageOpen || invoice == null) return;
            _isDetailedPageOpen = true;
            await _navigation.PushModalAsync(new OrderDetailPage(invoice));
        }

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
            // Position lastKnownPosition = new Position(-46.3, 168);
            if (_waypoints.Count > 0 && App.CheckIfInternet()) {
                
                _invoicesCollection.Clear();
                
                if(_waypoints.Count > MAX_WAYPOINTS) { 
                    GoogleMapsAPI.SortWaypoints(lastKnownPosition,_waypoints.ToArray());
                    List<Invoice> tempInvoice = new List<Invoice>();
                    for(int i = 0; i < GoogleMapsAPI._waypointsOrder.Count; i++)
                    {
                        tempInvoice.Add(_invoices[GoogleMapsAPI._waypointsOrder[i]]);
                    }
                    _invoices = tempInvoice;
                }
                
                for (int i = 0; i < _waypoints.Count; i+= MAX_WAYPOINTS)
                {
                    string destinationWaypoint;
                    int waypointCountLeft = _waypoints.Count - i;
                    string[] orderedWaypoints;
                    if (waypointCountLeft > MAX_WAYPOINTS) { 
                        orderedWaypoints = new string[MAX_WAYPOINTS];
                        _waypoints.CopyTo(i, orderedWaypoints, 0, MAX_WAYPOINTS);
                        destinationWaypoint = orderedWaypoints[orderedWaypoints.Count() - 1];
                    } else
                    {
                        orderedWaypoints = new string[waypointCountLeft];
                        _waypoints.CopyTo(i, orderedWaypoints, 0, waypointCountLeft);
                        if ((destinationWaypoint = Preferences.Get("EndPointGeoWaypoint", string.Empty)) == "")
                        {
                            destinationWaypoint = orderedWaypoints[orderedWaypoints.Count() - 1];
                        }
                    }

                    
                    _direction = await GoogleMapsAPI.MapDirectionsWithWaypoints(lastKnownPosition, destinationWaypoint, orderedWaypoints);
                    string[] newCurrentPosition = orderedWaypoints[orderedWaypoints.Count() - 1].Split( new string[] { "%2C" },StringSplitOptions.None);
                    lastKnownPosition = new Position(Convert.ToDouble(newCurrentPosition[0]), Convert.ToDouble(newCurrentPosition[1]));

                    //Only create a line if it returns something from google
                    if (_direction.Status != "ZERO_RESULTS" && _direction.Routes.Count > 0)
                    {
                        AddPolyLine();

                        foreach (int order in GoogleMapsAPI._waypointsOrder)
                        {
                            _invoicesCollection.Add(_invoices[order + i]);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Oops", "Unable to map the directions, Please make sure the address entered is legit", "OK");
                    }
                }
                DeliveryItemView.ItemsSource = _invoicesCollection;
            } 
            else if (_waypoints.Count() == 0 && App.CheckIfInternet())
            {
                string destinationPoint;
                if ((destinationPoint = Preferences.Get("EndPointGeoWaypoint", string.Empty)) == "") return; 
                _direction = await GoogleMapsAPI.MapDirectionsNoWaypoints(lastKnownPosition,destinationPoint);
                if (_direction.Status != "ZERO_RESULTS" && _direction.Routes.Count > 0)
                {
                    AddPolyLine();
                }
            }
        }

        private void AddPolyLine()
        {
            _legs.Clear();
            //List<Step> steps = new List<Step>();
            List<Position> directionPolylines = new List<Position>();
            //int iteration = 0;
            foreach (Leg leg in _direction.Routes[0].Legs)
            {
                if (leg.StartAddress == leg.EndAddress) continue;
                for (int i=0; i < leg.Steps.Count ; i++)
                {
                    directionPolylines = directionPolylines.Concat(PolylineHelper.Decode(leg.Steps[i].Polyline.Points).ToList()).ToList();
                    if(i != leg.Steps.Count - 1)
                    {
                        if(leg.Steps[i].Distance.Text.IndexOf(" km") != -1)
                        {
                            leg.Steps[i].Distance.Text = leg.Steps[i].Distance.Text.Replace("km", "kilometers");
                        } else if (leg.Steps[i].Distance.Text.IndexOf(" m") != -1)
                        {
                            leg.Steps[i].Distance.Text = leg.Steps[i].Distance.Text.Replace("m", "meters");
                        }
                        leg.Steps[i].HtmlInstructions += $" for {leg.Steps[i].Distance.Text}";
                    }
                    //steps.Add(leg.Steps[i]);
                }
                _legs.Add(leg);
            }
            CreatePolylinesOnMap(directionPolylines);
        }

        private void CreatePolylinesOnMap(List<Position> directionPolylines)
        {
            try {
                Xamarin.Forms.GoogleMaps.Polyline polyline = new Xamarin.Forms.GoogleMaps.Polyline()
                {
                    StrokeColor = Constants.mapShapeColor,
                    StrokeWidth = 8,
                };

                for (int i = 0; i < directionPolylines.Count; i++)
                {
                    polyline.Positions.Add(directionPolylines[i]);
                }

                if (polyline.Positions.Count == 0) return;
                map.Polylines.Add(polyline);
            } catch
            {
                Console.WriteLine("Error in polyline");
            }
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
            // Position lastKnownPosition = new Position(-46.3, 168);
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
            _currentSelectedInvoice = invoiceSelected;
            if (_isDetailedPageOpen) return;
            _isDetailedPageOpen = true;
            await _navigation.PushModalAsync(new OrderDetailPage(invoiceSelected));
        }

        private async void MarkAsCompleted(object sender, EventArgs e)
        {
            bool userAction = await DisplayAlert("Confirm action", "Do you wish to mark it as complete? ", "Yes", "Cancel");

            if (userAction)
            {
                var button = sender as Button;
               
                Invoice invoiceSelected = button.BindingContext as Invoice;


                InvoiceSQLite invoice = App.InvoiceDatabase.GetInvoiceByInvoiceID(invoiceSelected.InvoiceID);
                invoice.CompletedDeliveryStatus = true;
                invoice.UpdateTimeTicksApp = DateTime.Now.Ticks;

                App.InvoiceDatabase.UpdateInvoiceStatus(invoice);
                UpdateStatus(invoiceSelected);
                UpdateWeightLabel(invoiceSelected);

                if (IsGoogleAPICallRequired(invoiceSelected)) MapDirections("(Force-Refresh) ");
            }
        } // Mark As Completed

        public bool IsGoogleAPICallRequired(Invoice invoiceSelected)
        {
            bool googleAPICallRequired = true;
            InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceNumber(invoiceSelected.InvoiceNumber);
            if (invoiceSQLite != null)
            {
                ContactSQLite contact = App.ContactDatabase.GetContactByID(invoiceSQLite.ContactID);
                double distanceToInvoiceInKm = Location.CalculateDistance(_currentLocation, new Location((double)contact.Latitude, (double)contact.Longitude), DistanceUnits.Kilometers);

                if (distanceToInvoiceInKm < 0.045) googleAPICallRequired = false;
            }
            return googleAPICallRequired;
        }

        public void UpdateStatus(Invoice invoiceSelected)
        {
            _invoicesCollection.Remove(invoiceSelected);
            _invoices.Remove(invoiceSelected);
            DeliveryItemView.ItemsSource = _invoicesCollection;
            Pin thePin = _pins.Where(pinX => pinX.Tag as string == invoiceSelected.InvoiceID).FirstOrDefault();
            if(thePin != null) { 
                _waypoints.Remove($"{thePin.Position.Latitude}%2C{thePin.Position.Longitude}");
                map.Pins.Remove(thePin);
            }
            if(_legs.Count > _currentLegs) {
                _currentLegs++;
            }
            _counter--;
        }

        public void UpdateWeightLabel(Invoice invoiceSelected)
        {
            InvoiceSQLite invoiceSQLite = App.InvoiceDatabase.GetInvoiceByInvoiceID(invoiceSelected.InvoiceID);
            List<InvoiceSQLite> temp = new List<InvoiceSQLite>();
            temp.Add(invoiceSQLite);

            double invoiceWeight = StockManager.CalculateStockWeight(StockManager.GetStock(temp));

            if(invoiceSQLite.InvoiceType == "ACCREC")
            {
                _currentWeight -= invoiceWeight;
            } else
            {
                _currentWeight += invoiceWeight;
            }
            TotalWeightLabel.Text = $"{(_currentWeight):F2} Kg";
        }

        private void ImgClose_Tapped(object sender, EventArgs e)
        {
            _navigation.PopAsync(true);
        }
    }
}