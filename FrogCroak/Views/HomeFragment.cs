using System.Collections.Generic;
using Android.OS;
using Android.Views;
using Android.Support.V4.App;
using Android.Support.V4.View;
using FrogCroak.MyMethod;
using Android.Support.Design.Widget;

namespace FrogCroak.Views
{
    public class HomeFragment : Fragment
    {
        MainActivity mainActivity;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View view = inflater.Inflate(Resource.Layout.HomeFragment, container, false);
            mainActivity = (MainActivity)Activity;
            initView(view);
            return view;
        }

        private void initView(View view)
        {
            Android.Support.V7.Widget.Toolbar toolbar = (Android.Support.V7.Widget.Toolbar)view.FindViewById(Resource.Id.toolbar);
            mainActivity.SetSupportActionBar(toolbar);
            mainActivity.SupportActionBar.SetDisplayShowTitleEnabled(false);
            List<Fragment> fragments = new List<Fragment>();
            fragments.Add(new MapFragment());
            fragments.Add(new ChatFragment());
            fragments.Add(new SoundRecognitionFragment());

            ViewPager viewPager = (ViewPager)view.FindViewById(Resource.Id.viewpager);
            ViewPagerAdapter viewPagerAdapter = new ViewPagerAdapter(ChildFragmentManager, fragments, Activity);
            viewPagerAdapter.tabTitles = new string[] { "", "", "" };
            viewPagerAdapter.tabIcons = new int[]{
                Resource.Drawable.map,
                Resource.Drawable.chat,
                Resource.Drawable.frog
            };

            viewPager.Adapter = viewPagerAdapter;
            viewPager.OffscreenPageLimit = 2;

            TabLayout tabs = (TabLayout)view.FindViewById(Resource.Id.tabs);
            tabs.TabMode = TabLayout.ModeFixed;
            tabs.TabGravity = TabLayout.GravityFill;
            tabs.SetupWithViewPager(viewPager);
        }
    }
}