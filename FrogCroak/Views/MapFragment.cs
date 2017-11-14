using System;
using Android.OS;
using Android.Views;
using Android.Support.V4.App;
using Android.Gms.Maps;
using FrogCroak.MyMethod;
using Android.Gms.Maps.Model;
using FrogCroakCL.Models;
using FrogCroakCL.Services;
using System.Threading.Tasks;
using static FrogCroakCL.Models.MyMarkers;
using System.Net;
using System.Collections.Generic;

namespace FrogCroak.Views
{
    public class MapFragment : Fragment
    {
        private MapView mMapView;
        public GoogleMap googleMap;
        private MainActivity mainActivity;

        private MarkerService markerService;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.MapFragment, container, false);
            mainActivity = (MainActivity)Activity;

            markerService = new MarkerService();
            mMapView = (MapView)view.FindViewById(Resource.Id.mapView);
            mMapView.OnCreate(savedInstanceState);

            mMapView.OnResume(); // needed to get the map to display immediately

            try
            {
                MapsInitializer.Initialize(Activity.ApplicationContext);
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }

            mMapView.GetMapAsync(new VeryNaiveImpl
            {
                Callback = (map) =>
                {
                    googleMap = map;
                    CustomInfoWindowAdapter adapter = new CustomInfoWindowAdapter(mainActivity, this);
                    googleMap.SetInfoWindowAdapter(adapter);
                    googleMap.Clear();
                    LatLng sydney = new LatLng(23.674764, 120.796819);
                    CameraPosition cameraPosition = new CameraPosition.Builder().Target(sydney).Zoom(7.3f).Build();
                    googleMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
                    DrawMap();
                }
            });
            return view;
        }

        public async void DrawMap()
        {
            if (mainActivity.sp_Settings.GetBoolean("IsShowMyMarker", true))
            {
                List<UserMarker> UserMarkerList = markerService.GetUserMarkerList();
                LatLng sydney;
                foreach (UserMarker userMarker in UserMarkerList)
                {
                    sydney = new LatLng(userMarker.Latitude, userMarker.Longitude);
                    googleMap.AddMarker(new MarkerOptions()
                            .SetPosition(sydney)
                            .SetTitle(userMarker.Title)
                            .SetSnippet(userMarker.Content)
                            .SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.personal))
                    );
                }
            }

            if (mainActivity.sp_Settings.GetBoolean("IsShowAllMarker", true))
            {
                AllRequestResult result = null;
                await Task.Run(() =>
                {
                    result = new MarkerService().GetMyMarkerList();
                });
                if (result.IsSuccess)
                {
                    MyMarkers myMarkers = (MyMarkers)result.Result;
                    LatLng sydney = new LatLng(0, 0);
                    foreach (MyMarker myMarker in myMarkers.MarkerList)
                    {
                        sydney = new LatLng(myMarker.Latitude, myMarker.Longitude);
                        myMarker.Content = myMarker.Content.Replace("\\n", "\n");
                        googleMap.AddMarker(new MarkerOptions()
                                .SetPosition(sydney)
                                .SetTitle(myMarker.Title)
                                .SetSnippet(myMarker.Content)
                                .SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.normal))
                        );
                    }
                    CameraPosition cameraPosition = new CameraPosition.Builder().Target(sydney).Zoom(7.3f).Build();
                    googleMap.AnimateCamera(CameraUpdateFactory.NewCameraPosition(cameraPosition));
                }
                else
                {
                    SharedService.WebExceptionHandler((WebException)result.Result, mainActivity);
                }
            }

        }
    }
}