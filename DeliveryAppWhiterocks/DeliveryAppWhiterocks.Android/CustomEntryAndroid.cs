using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using DeliveryAppWhiterocks.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using DeliveryAppWhiterocks.ViewModels;
using Android.Text;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryAndroid))]
namespace DeliveryAppWhiterocks.Droid
{
    class CustomEntryAndroid : EntryRenderer
    {
        public CustomEntryAndroid(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                //this.Control.SetRawInputType(InputTypes.TextFlagNoSuggestions);
                Control.SetBackgroundColor(global::Android.Graphics.Color.White);
                Control.SetHintTextColor(ColorStateList.ValueOf(global::Android.Graphics.Color.LightGray));
            }
        }
    }
}