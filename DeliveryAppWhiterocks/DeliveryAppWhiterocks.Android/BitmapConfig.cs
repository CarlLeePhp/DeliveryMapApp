using DeliveryAppWhiterocks.Droid;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Forms.GoogleMaps.Android.Factories;
using AndroidBitmapDescriptor = Android.Gms.Maps.Model.BitmapDescriptor;
using AndroidBitmapDescriptorFactory = Android.Gms.Maps.Model.BitmapDescriptorFactory;

public sealed class BitmapConfig : IBitmapDescriptorFactory
{
    public AndroidBitmapDescriptor ToNative(BitmapDescriptor descriptor)
    {
        int iconId = 0;
        switch (descriptor.Id)
        {
            case "ACCREC":
                iconId = Resource.Drawable.DropOffMarker;
                break;
            case "ACCPAY":
                iconId = Resource.Drawable.PickupMarker;
                break;
        }
        return AndroidBitmapDescriptorFactory.FromResource(iconId);
    }
}