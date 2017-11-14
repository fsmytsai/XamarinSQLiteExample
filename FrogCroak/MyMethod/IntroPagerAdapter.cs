using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Support.V4.View;
using FrogCroak.Views;
using Java.Lang;
using Java.IO;

namespace FrogCroak.MyMethod
{
    public class IntroPagerAdapter : PagerAdapter
    {
        private List<View> viewList;
        private MainActivity mainActivity;

        public IntroPagerAdapter(List<View> mViewList, MainActivity mMainActivity)
        {
            this.viewList = mViewList;
            mainActivity = mMainActivity;
        }

        public override void DestroyItem(ViewGroup container, int position, Object @object)
        {
            container.RemoveView((View)@object);
        }

        public override Object InstantiateItem(ViewGroup container, int position)
        {
            View view = viewList[position];
            if (position == 3)
            {

                ISharedPreferences sp_Settings = mainActivity.GetSharedPreferences("Settings", FileCreationMode.Private);
                CheckedTextView ctv_NeverIntro = (CheckedTextView)view.FindViewById(Resource.Id.ctv_NeverIntro);
                ctv_NeverIntro.Checked = sp_Settings.GetBoolean("IsNeverIntro", false);
                ctv_NeverIntro.Click += delegate
                {
                    ctv_NeverIntro.Toggle();
                };

                Button bt_StartConGroup = (Button)view.FindViewById(Resource.Id.bt_StartConGroup);
                bt_StartConGroup.Click += delegate
                {
                    bool root = isRoot();
                    bool fromGooglePlay = isFromGooglePlay();
                    string ErrorMessage = "";
                    if (root && !fromGooglePlay)
                    {
                        ErrorMessage = "您不但 Root 且還不是從 Google Play 安裝，壞透了，不給你用。";
                    }
                    //else if (root)
                    //{
                    //    ErrorMessage = "您的手機已 Root ，無法使用本程式。";
                    //}
                    //else if (!fromGooglePlay)
                    //{
                    //    ErrorMessage = "您並非使用 Google Play 安裝，無法使用本程式。";
                    //}

                    if (ErrorMessage == "")
                    {
                        sp_Settings.Edit().PutBoolean("IsNeverIntro", ctv_NeverIntro.Checked).Apply();
                        mainActivity.SupportFragmentManager
                                .BeginTransaction()
                                .Replace(Resource.Id.MainFrameLayout, new HomeFragment(), "HomeFragment")
                                .Commit();
                    }
                    else
                    {
                        new AlertDialog.Builder(mainActivity)
                                .SetTitle("錯誤")
                                .SetMessage(ErrorMessage)
                                .SetIcon(Resource.Drawable.Icon)
                                .SetNegativeButton("QQ", delegate
                                {
                                })
                                .Show();
                    }
                };
            }
            container.AddView(view);
            return view;
        }

        public override int Count => viewList.Count;

        public override bool IsViewFromObject(View view, Object @object)
        {
            return view == @object;
        }

        public bool isRoot()
        {
            string binPath = "/system/bin/su";
            string xBinPath = "/system/xbin/su";
            if (new File(binPath).Exists() && isExecutable(binPath))
                return true;
            if (new File(xBinPath).Exists() && isExecutable(xBinPath))
                return true;
            return false;
        }

        private bool isExecutable(string filePath)
        {
            Process p = null;
            try
            {
                p = Runtime.GetRuntime().Exec("ls -l " + filePath);
                BufferedReader br = new BufferedReader(new InputStreamReader(p.InputStream));
                string str = br.ReadLine();
                if (str != null && str.Length >= 4)
                {
                    char flag = str[3];
                    if (flag == 's' || flag == 'x')
                        return true;
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (p != null)
                {
                    p.Destroy();
                }
            }
            return false;
        }

        public bool isFromGooglePlay()
        {
            string InstallerPackageName = mainActivity.PackageManager.GetInstallerPackageName(mainActivity.PackageName);
            if (InstallerPackageName != null)
                return InstallerPackageName == "com.android.vending";
            return false;
        }
    }
}