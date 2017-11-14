using Android.Widget;
using Android.OS;
using Android.Content.PM;
using FrogCroak.MyMethod;
using Android.Support.V7.App;
using Android.Views;
using Android.Content;
using Android.App;
using FrogCroakCL.Services;

namespace FrogCroak.Views
{
    [Activity(ScreenOrientation = ScreenOrientation.Portrait, MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity
    {
        public View activity_Outer;

        public const int AddMarker_Code = 20;

        public ISharedPreferences sp_Settings;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            sp_Settings = GetSharedPreferences("Settings", FileCreationMode.Private);
            if (sp_Settings.GetBoolean("IsNeverIntro", false))
            {
                SupportFragmentManager
                    .BeginTransaction()
                    .Replace(Resource.Id.MainFrameLayout, new HomeFragment(), "HomeFragment")
                    .Commit();
            }
            else
            {
                SupportFragmentManager
                    .BeginTransaction()
                    .Replace(Resource.Id.MainFrameLayout, new IntroductionFragment(), "IntroductionFragment")
                    .Commit();
            }
            activity_Outer = FindViewById(Resource.Id.Activity_Outer);
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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            IMenuItem item = menu.FindItem(Resource.Id.item_ShowMyMarker);
            item.SetChecked(sp_Settings.GetBoolean("IsShowMyMarker", true));
            item = menu.FindItem(Resource.Id.item_ShowAllMarker);
            item.SetChecked(sp_Settings.GetBoolean("IsShowAllMarker", true));
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.item_AddMarker)
            {
                Intent intent = new Intent(this, typeof(AddMarkerActivity));
                StartActivityForResult(intent, AddMarker_Code);
            }
            else if (item.ItemId == Resource.Id.item_ShowMyMarker)
            {
                item.SetChecked(!item.IsChecked);
                sp_Settings.Edit().PutBoolean("IsShowMyMarker", item.IsChecked).Apply();
                HomeFragment homeFragment = (HomeFragment)SupportFragmentManager.FindFragmentByTag("HomeFragment");
                MapFragment mapFragment = (MapFragment)homeFragment.ChildFragmentManager.FindFragmentByTag("android:switcher:" + Resource.Id.viewpager + ":0");
                mapFragment.googleMap.Clear();
                mapFragment.DrawMap();
            }
            else if (item.ItemId == Resource.Id.item_ShowAllMarker)
            {
                item.SetChecked(!item.IsChecked);
                sp_Settings.Edit().PutBoolean("IsShowAllMarker", item.IsChecked).Apply();
                HomeFragment homeFragment = (HomeFragment)SupportFragmentManager.FindFragmentByTag("HomeFragment");
                MapFragment mapFragment = (MapFragment)homeFragment.ChildFragmentManager.FindFragmentByTag("android:switcher:" + Resource.Id.viewpager + ":0");
                mapFragment.googleMap.Clear();
                mapFragment.DrawMap();
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok && requestCode == AddMarker_Code)
            {
                HomeFragment homeFragment = (HomeFragment)SupportFragmentManager.FindFragmentByTag("HomeFragment");
                MapFragment mapFragment = (MapFragment)homeFragment.ChildFragmentManager.FindFragmentByTag("android:switcher:" + Resource.Id.viewpager + ":0");
                mapFragment.googleMap.Clear();
                mapFragment.DrawMap();
            }
        }
    }
}