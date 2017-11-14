using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using FrogCroak.MyMethod;
using Android.Support.V7.App;
using Android.Content.PM;
using Android.Locations;
using Android.Runtime;
using Android.Views;
using Android.Support.V4.App;
using FrogCroakCL.Services;
using FrogCroakCL.Models;

namespace FrogCroak.Views
{
    [Activity(Label = "AddMarkerActivity")]
    public class AddMarkerActivity : AppCompatActivity
    {
        public View activity_Outer;

        private MarkerService markerService;

        private ImageView iv_AddMarker;
        private EditText et_Title;
        private EditText et_Content;
        private TextView tv_Latitude;
        private TextView tv_Longitude;

        private const int REQUEST_LOCATION = 19;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.AddMarkerActivity);
            Android.Support.V7.Widget.Toolbar toolbar = (Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            activity_Outer = FindViewById(Resource.Id.Activity_Outer);
            markerService = new MarkerService();
            initView();
            if (ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessFineLocation) == (int)Permission.Granted &&
                ActivityCompat.CheckSelfPermission(this, Android.Manifest.Permission.AccessCoarseLocation) == (int)Permission.Granted)
            {
                locationServiceInitial();
            }
            else
            {
                RequestPermissions(new string[] {
                    Android.Manifest.Permission.AccessFineLocation,
                    Android.Manifest.Permission.AccessCoarseLocation
                }, REQUEST_LOCATION);
            }
        }

        public void initView()
        {
            et_Title = FindViewById<EditText>(Resource.Id.et_Title);
            et_Content = FindViewById<EditText>(Resource.Id.et_Content);
            tv_Latitude = FindViewById<TextView>(Resource.Id.tv_Latitude);
            tv_Longitude = FindViewById<TextView>(Resource.Id.tv_Longitude);
            iv_AddMarker = FindViewById<ImageView>(Resource.Id.iv_AddMarker);
            iv_AddMarker.Click += delegate
            {
                AddMarker();
            };
        }

        public override bool DispatchTouchEvent(MotionEvent e)
        {
            View v = CurrentFocus;
            if (v is EditText)
            {
                View w = CurrentFocus;
                int[] scrcoords = new int[2];
                w.GetLocationOnScreen(scrcoords);
                float x = e.RawX + w.Left - scrcoords[0];
                float y = e.RawY + w.Top - scrcoords[1];
                if (e.Action == MotionEventActions.Up
                && (x < w.Left || x >= w.Right || y < w.Top || y > w.Bottom))
                {
                    SharedService.HideKeyboard(this);
                    activity_Outer.RequestFocus();
                }
            }
            return base.DispatchTouchEvent(e);
        }

        public override Intent SupportParentActivityIntent => finish();

        public Intent finish()
        {
            Finish();
            return null;
        }

        private LocationManager lms;
        private String bestProvider = LocationManager.GpsProvider; //最佳資訊提供者
        private void locationServiceInitial()
        {
            lms = (LocationManager)GetSystemService(LocationService);  //取得系統定位服務
            Criteria criteria = new Criteria(); //資訊提供者選取標準
            bestProvider = lms.GetBestProvider(criteria, true); //選擇精準度最高的提供者
            Location location = lms.GetLastKnownLocation(bestProvider);
            if (location == null)
            {
                SharedService.ShowTextToast("取得位置失敗", this);
                Finish();
            }
            else
            {
                tv_Latitude.Text = location.Latitude.ToString();
                tv_Longitude.Text = location.Longitude.ToString();
            }
        }

        public void AddMarker()
        {
            if (!tv_Latitude.Text.Equals(""))
            {
                string Title = et_Title.Text.Trim();
                string Content = et_Content.Text.Trim();

                if (Title != "" && Content != "")
                {
                    var userMarker = new UserMarker();
                    userMarker.Latitude = Double.Parse(tv_Latitude.Text);
                    userMarker.Longitude = Double.Parse(tv_Longitude.Text);
                    userMarker.Title = et_Title.Text;
                    userMarker.Content = et_Content.Text;

                    bool IsSuccess = markerService.CreateUserMarker(userMarker);
                    if (IsSuccess)
                    {
                        SharedService.ShowTextToast("新增位置標記成功", this);
                        SetResult(Result.Ok);
                        Finish();
                    }
                    else
                    {
                        SharedService.ShowTextToast("新增位置標記失敗", this);
                    }
                }
                else
                {
                    SharedService.ShowTextToast("請完整填寫資料", this);
                }

            }
            else
            {
                SharedService.ShowTextToast("取得位置失敗", this);
            }

        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == REQUEST_LOCATION)
            {
                if (grantResults.Length == 2 && grantResults[0] == (int)Permission.Granted && grantResults[1] == (int)Permission.Granted)
                {
                    locationServiceInitial();
                }
                else
                {
                    SharedService.ShowTextToast("您拒絕了提供位置資訊", this);
                    Finish();
                }
            }
        }
    }
}