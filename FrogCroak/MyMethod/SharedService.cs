using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.Content.Res;
using Android.Net;
using Android.Views.InputMethods;
using Android.Graphics;
using System.IO;
using Android.Media;
using System.Net;

namespace FrogCroak.MyMethod
{
    public class SharedService
    {
        public static int getActionBarSize(Context context)
        {
            TypedArray styledAttributes = context.Theme.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ActionBarSize });
            int actionBarSize = (int)styledAttributes.GetDimension(0, 0);
            styledAttributes.Recycle();
            return actionBarSize;
        }

        //避免重複Toast
        private static Toast toast = null;

        public static void ShowTextToast(String msg, Context context)
        {
            if (toast == null)
            {
                toast = Toast.MakeText(context, msg, ToastLength.Short);
            }
            else
            {
                toast.SetText(msg);
            }
            toast.Show();
        }

        public static bool CheckNetWork(Context context)
        {
            ConnectivityManager connManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            NetworkInfo networkInfo = connManager.ActiveNetworkInfo;
            if (networkInfo == null || !networkInfo.IsConnected || !networkInfo.IsAvailable)
            {
                ShowTextToast("請檢察網路連線", context);
                return false;
            }
            return true;
        }

        //關閉鍵盤
        public static void HideKeyboard(Activity activity)
        {
            InputMethodManager inputMethodManager =
                    (InputMethodManager)activity.GetSystemService(
                            Activity.InputMethodService);

            try
            {
                inputMethodManager.HideSoftInputFromWindow(
                        activity.CurrentFocus.WindowToken, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static Bitmap CompressImage(string FromPath, string ToPath)
        {
            BitmapFactory.Options BFOptions = new BitmapFactory.Options();
            BFOptions.InJustDecodeBounds = true;
            Bitmap image = BitmapFactory.DecodeFile(FromPath, BFOptions);
            BFOptions.InJustDecodeBounds = false;
            int be = 0;
            if (BFOptions.OutWidth > BFOptions.OutHeight)
                be = (int)(BFOptions.OutWidth / (float)720);
            else
                be = (int)(BFOptions.OutHeight / (float)720);
            if (be <= 0)
                be = 1;
            BFOptions.InSampleSize = be;
            image = BitmapFactory.DecodeFile(FromPath, BFOptions);
            int w = image.Width;
            int h = image.Height;

            var baos = new MemoryStream();
            image.Compress(Bitmap.CompressFormat.Jpeg, 100, baos);//品質壓縮方法，這裡100表示不壓縮，把壓縮後的資料存放到baos中
            int options = 100;
            Android.Util.Log.Debug("baos.Length=", baos.Length + "");
            while (baos.Length / 1024 > 100)
            { //迴圈判斷如果壓縮後圖片大於200kb則繼續壓縮
                baos.SetLength(0);//重置baos即清空baos
                options -= 10;//每次都減少10
                image.Compress(Bitmap.CompressFormat.Jpeg, options, baos);//這裡壓縮options%，把壓縮後的資料存放到baos中

                Android.Util.Log.Debug("baos.Length=", baos.Length + "");
                if (options == 0)
                    break;
            }
            try
            {
                int degree = 0;
                ExifInterface exifInterface = new ExifInterface(FromPath);
                int orientation = exifInterface.GetAttributeInt(ExifInterface.TagOrientation, (int)Android.Media.Orientation.Normal);
                switch (orientation)
                {
                    case (int)Android.Media.Orientation.Rotate90:
                        degree = 90;
                        break;
                    case (int)Android.Media.Orientation.Rotate180:
                        degree = 180;
                        break;
                    case (int)Android.Media.Orientation.Rotate270:
                        degree = 270;
                        break;
                }
                if (degree != 0)
                {
                    var stream = new MemoryStream(baos.ToArray());

                    image = BitmapFactory.DecodeStream(stream, null, null);
                    Matrix matrix = new Matrix();
                    matrix.PostRotate(degree);
                    int width = image.Width;
                    int height = image.Height;
                    image = Bitmap.CreateBitmap(image, 0, 0, width, height, matrix, true);
                }

                FileStream outfile = new FileStream(ToPath, FileMode.Create);
                image.Compress(Bitmap.CompressFormat.Jpeg, 100, outfile);

                outfile.Close();
                baos.Close();

                return image;
            }
            catch (Java.IO.FileNotFoundException e)
            {
                // TODO Auto-generated catchblock
                e.PrintStackTrace();
                return null;
            }
            catch (Java.IO.IOException e)
            {
                // TODO Auto-generated catchblock
                e.PrintStackTrace();
                return null;
            }
        }

        public static void WebExceptionHandler(WebException exception, Context mContext)
        {
            if (exception.Status == WebExceptionStatus.ProtocolError && exception.Response != null)
            {
                var response = (HttpWebResponse)exception.Response;
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var content = reader.ReadToEnd();
                        ShowTextToast(content, mContext);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    new AlertDialog.Builder(mContext)
                            .SetTitle("錯誤")
                            .SetMessage("伺服器錯誤，請聯絡開發人員")
                            .SetIcon(Resource.Drawable.Icon)
                            .SetPositiveButton("好的", delegate
                            {
                            })
                            .Show();
                }
            }
            else
            {
                ShowTextToast("請檢察網路連線", mContext);
            }
        }
    }
}