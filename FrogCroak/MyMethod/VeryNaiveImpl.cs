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
using Android.Gms.Maps;

namespace FrogCroak.MyMethod
{
    public class VeryNaiveImpl : Java.Lang.Object, IOnMapReadyCallback
    {
        public Action<GoogleMap> Callback;

        public void OnMapReady(GoogleMap googleMap) { if (Callback != null) Callback(googleMap); }
    }

}