using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Java.Lang;
using Android.Text;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Text.Style;

namespace FrogCroak.MyMethod
{
    public class ViewPagerAdapter : FragmentPagerAdapter
    {
        public string[] tabTitles;
        public int[] tabIcons;
        private List<Fragment> fragments;
        private Context context;

        public ViewPagerAdapter(FragmentManager fm, List<Fragment> f, Context mContext)
            : base(fm)
        {
            fragments = f;
            context = mContext;
        }

        public override int Count
        {
            get { return fragments.Count; }
        }

        public override Fragment GetItem(int position)
        {
            return fragments[position];
        }

        public override ICharSequence GetPageTitleFormatted(int position)
        {
            //return base.GetPageTitleFormatted(position);
            if (tabIcons == null)
            {
                return new Java.Lang.String(tabTitles[position]);
            }
            else
            {
                if (tabTitles[position] != "")
                    return new Java.Lang.String(tabTitles[position]);
                SpannableString spannableString;

                Drawable drawable = ContextCompat.GetDrawable(context, tabIcons[position]);

                int height = (int)(SharedService.getActionBarSize(context) * 0.7);

                drawable.SetBounds(0, 0, height, height);
                ImageSpan imageSpan = new ImageSpan(drawable);
                //to make our tabs icon only, set the Text as blank string with white space
                spannableString = new SpannableString(" ");
                spannableString.SetSpan(imageSpan, 0, spannableString.Length(), SpanTypes.ExclusiveExclusive);
                return spannableString;
            }
        }
    }
}