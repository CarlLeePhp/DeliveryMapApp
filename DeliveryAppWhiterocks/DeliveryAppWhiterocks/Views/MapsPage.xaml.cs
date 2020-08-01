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

        CompositeDisposable _EventSubscriptions = new CompositeDisposable();
        PanGestureRecognizer _panGesture = new PanGestureRecognizer();

        double _transY;

        ObservableCollection<Invoice> _invoicesCollection = new ObservableCollection<Invoice>();

        //remove the passing parameter later. now is used only for testing
        public MapsPage(ObservableCollection<Invoice> invoices)
        {
            InitializeComponent();
            _invoicesCollection = invoices;
            //replace this later, get data from GoogleAPI.Waypoints after sorted.
            DeliveryItemView.ItemsSource = _invoicesCollection;

            //Enable the blue circle that mark the current location of user
            map.MyLocationEnabled = true;
            InitMap();

            //Got the information from https://winstongubantes.blogspot.com/2017/11/creating-draggable-sliding-up-panel-in.html
            CollapseAllMenus(); 
            InitializeObservables();
        }

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

        public async void InitMap()
        {
            CenterMapToCurrentLocation();
            //await InitPins();
            //MapWaypoints();
            //The Geocoder.GetPositionsForAddressAsync() doesnt work, it shows GRPC error,

            //Position position = await GoogleMapsAPI.GetPositionFromKnownAddress("84 Lithgow st, Invercargill ,New Zealand");

            //if (position.Latitude == 0 && position.Longitude == 0)
            //{
            //    await DisplayAlert("Alert", "Not Found", "OK");
            //}
            //else
            //{
            //    map.MyLocationEnabled = true;
            //    map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(3)), true);

            //    string reverseGeo = await GoogleMapsAPI.GetAdressFromKnownPosition(position);
            //    await DisplayAlert("Map", $"{reverseGeo}", "OK");
            //}
        }


        private async void MapWaypoints()
        {
            GoogleDirection direction = await GoogleMapsAPI.MapDirections(_lastKnownPosition, _waypoints.ToArray());
            List<Position> directionPolylines = PolylineHelper.Decode(direction.Routes[0].OverviewPolyline.Points).ToList();
            for (int i = 0; i < directionPolylines.Count - 1; i++)
            {
                Xamarin.Forms.GoogleMaps.Polyline polyline = new Xamarin.Forms.GoogleMaps.Polyline()
                {
                    StrokeColor = Constants.mapShapeColor,
                    StrokeWidth = 8,
                };

                polyline.Positions.Add(directionPolylines[i]);
                polyline.Positions.Add(directionPolylines[i + 1]);
                map.Polylines.Add(polyline);
            }
        }

        private async Task<bool> InitPins()
        {
            foreach (InvoiceSQLite invoice in App.InvoiceDatabase.GetAllIncompleteInvoices())
            {
                ContactSQLite customerContact = App.ContactDatabase.GetContactByID(invoice.ContactID);
                string fullAddress = customerContact.Address;

                if (customerContact.City != "")
                {
                    fullAddress += $", {customerContact.City}";
                }
                fullAddress += $", New Zealand";

                Position position = await GoogleMapsAPI.GetPositionFromKnownAddress(fullAddress);
                //separate waypoints by comma
                _waypoints.Add($"{position.Latitude}%2C{position.Longitude}");
                var pin = new Pin()
                {
                    Position = position,
                    Label = $"{invoice.InvoiceNumber}",
                    //set tag so i can reference it when a pin is clicked
                    Tag = invoice
                };

                map.SelectedPinChanged += Map_SelectedPinChanged;

                map.Pins.Add(pin);
            }
            return true;
        }

        private void Map_SelectedPinChanged(object sender, SelectedPinChangedEventArgs e)
        {
            Pin currentPinSelected = e.SelectedPin;
        }

        private async void CenterMapToCurrentLocation()
        {
            Location lastKnownLocation = await Geolocation.GetLastKnownLocationAsync();
            _lastKnownPosition = new Position(lastKnownLocation.Latitude, lastKnownLocation.Longitude);
            map.MoveToRegion(new MapSpan(_lastKnownPosition, 0.05, 0.05));
        }

        private void currentLocationButton_Clicked(object sender, EventArgs e)
        {
            CenterMapToCurrentLocation();
        }


        private void SwipeGestureRecognizer_Swiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Up:
                    // Handle the swipe
                    //open the layout
                    DisplayAlert("OK", "SWIPED UP", "OK");
                    break;
                case SwipeDirection.Down:
                    // Handle the swipe
                    //close the layout
                    DisplayAlert("OK", "SWIPED DOWN", "OK");
                    break;
            }
        }

        private void ItemDetailsButton_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            Invoice invoiceSelected = button.BindingContext as Invoice;

            Navigation.PushModalAsync(new OrderDetailPage(invoiceSelected));
        }
    }
}