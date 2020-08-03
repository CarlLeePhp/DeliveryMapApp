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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
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
        Position _lastKnownPosition;
        Geocoder geocoder = new Geocoder();

        //properties for sliding up menu
        CompositeDisposable _EventSubscriptions = new CompositeDisposable();
        PanGestureRecognizer _panGesture = new PanGestureRecognizer();
        double _transY;
        //end sliding up menu props

        ObservableCollection<Invoice> _invoicesCollection = new ObservableCollection<Invoice>();
        List<Invoice> _invoices = new List<Invoice>();

        List<Pin> pins;

        //remove the passing parameter later. now is used only for testing
        public MapsPage(List<Invoice> invoices)
        {
            InitializeComponent();

            _invoices = invoices;

            //Enable the blue circle that mark the current location of user
            map.MyLocationEnabled = true;

            InitMap();

            //Got the information from https://winstongubantes.blogspot.com/2017/11/creating-draggable-sliding-up-panel-in.html
            CollapseAllMenus(); 
            InitializeObservables();
        }

        //SLIDEUP MENU REGION
        //a layout that helps in resizing the stacklayout that contains the delivery orders
        #region SlideUpMenu
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _EventSubscriptions.Clear();
        }

        private void CollapseAllMenus()
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

            _EventSubscriptions.Add(panGestureObservable);
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
                            QuickMenuPullLayout.TranslationY = Math.Max(0,
                                Math.Min(Notification.HeightRequest, QuickMenuPullLayout.TranslationY + e.TotalY));
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

        public async void InitMap()
        {
            CenterMapToCurrentLocation();
            await InitPins();
            MapWaypoints();
            //The Geocoder.GetPositionsForAddressAsync() doesnt work, it shows GRPC error, dont use it
        }

        private async Task<bool> InitPins()
        {
            pins = new List<Pin>();

            foreach (InvoiceSQLite invoice in App.InvoiceDatabase.GetAllIncompleteInvoices())
            {
                ContactSQLite customerContact = App.ContactDatabase.GetContactByID(invoice.ContactID);
                Position position;

                if (!customerContact.Latitude.HasValue) { 

                    //Get better format from address
                    #region Format the Address
                    string fullAddress = customerContact.Address;
                    if (customerContact.City != "")
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
                            position = (await geocoder.GetPositionsForAddressAsync(fullAddress)).FirstOrDefault();
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
                        Tag = invoice
                    };
                    pins.Add(pin);

                    map.SelectedPinChanged += Map_SelectedPinChanged;
                    map.Pins.Add(pin);
                    #endregion
                }
            }
            return true;
        }

        //InitPins() should be called before this method, _waypoints is added in InitPins()
        //A method that let's google handle the directions, shortest path etc.
        private async void MapWaypoints()
        {
            if(_waypoints.Count > 0 && App.CheckIfInternet()) {
                
                _invoicesCollection.Clear();
                GoogleDirection direction = await GoogleMapsAPI.MapDirections(_lastKnownPosition, _waypoints.ToArray());
                List<Position> directionPolylines = PolylineHelper.Decode(direction.Routes[0].OverviewPolyline.Points).ToList();

                //Create Polyline based on the decoded direction from google
                #region Create polylines on map
                for (int i = 0; i < directionPolylines.Count - 1; i++)
                {
                    Xamarin.Forms.GoogleMaps.Polyline polyline = new Xamarin.Forms.GoogleMaps.Polyline()
                    {
                        StrokeColor = Constants.mapShapeColor,
                        StrokeWidth = 8,
                    };

                    try { 
                        polyline.Positions.Add(directionPolylines[i]);
                        polyline.Positions.Add(directionPolylines[i + 1]);
                        map.Polylines.Add(polyline);
                    } catch
                    {
                        continue;
                    }
                }
                #endregion

                foreach(int order in GoogleMapsAPI._waypointsOrder)
                {
                    _invoicesCollection.Add(_invoices[order]);
                }
                //replace this later, get data from GoogleAPI.Waypoints after sorted.
                DeliveryItemView.ItemsSource = _invoicesCollection;
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
            Location lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
            _lastKnownPosition = new Position(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
            map.MoveToRegion(new MapSpan(_lastKnownPosition, 0.05, 0.05));
        }

        //tied to the XAML side, x:Name=currentLocationButton
        private void currentLocationButton_Clicked(object sender, EventArgs e)
        {
            CenterMapToCurrentLocation();
        }
        
        //When the see items button is clicked it will go to the order details page
        private void ItemDetailsButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            Invoice invoiceSelected = button.BindingContext as Invoice;

            Navigation.PushModalAsync(new OrderDetailPage(invoiceSelected));
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
                
                App.InvoiceDatabase.UpdateInvoiceStatus(invoice);

                _invoicesCollection.Remove(invoiceSelected);
                DeliveryItemView.ItemsSource = _invoicesCollection;

                Pin thePin = pins.Where(pinX => pinX.Label == invoiceSelected.InvoiceNumber).FirstOrDefault();
                map.Pins.Remove(thePin);
            }
        } // Mark As Completed
    }
}