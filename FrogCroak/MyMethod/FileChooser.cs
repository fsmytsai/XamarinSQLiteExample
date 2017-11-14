using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Android.Annotation;
using Android.Provider;
using Android.Net;
using Java.Lang;
using Android.Database;
using Android.Content.PM;
using Android.Support.V4.App;

namespace FrogCroak.MyMethod
{
    public class FileChooser
    {
        public static readonly int ACTIVITY_FILE_CHOOSER = 9973;

        // TODO -----物件變數-----

        /**
         * 儲存使用這個檔案選取器的Activity。
         */
        private readonly Android.App.Activity activity;
        private Fragment fragment;

        // TODO -----物件變數-----

        /**
         * 儲存是否正在選取檔案。
         */
        private bool choosing = false;
        /**
         * 儲存被選到的檔案是否一定要可以讀取。
         */
        private bool mustCanRead;
        /**
         * 儲存被選到的檔案。
         */
        private File[] chosenFiles;
        public Uri oneUri;

        // TODO -----建構子-----

        /**
         * 建構子，在Activity內使用檔案選取器。
         *
         * @param activity 傳入使用這個檔案選取器的Activity。
         */
        public FileChooser(Android.App.Activity activity)
        {
            this.activity = activity;
        }

        public FileChooser(Android.App.Activity activity, Fragment fragment)
        {
            this.activity = activity;
            this.fragment = fragment;
        }

        // TODO -----類別方法-----

        /**
         * 從Uri取得絕對路徑。
         *
         * @param context 傳入Context
         * @param uris    傳入Uri陣列
         * @return 傳回絕對路徑字串陣列，若絕對路徑無法取得，則對應的陣列索引位置為null
         */
        public static string[] getAbsolutePathsFromUris(Context context, Uri[] uris)
        {
            return getAbsolutePathsFromUris(context, uris, false);
        }

        /**
         * 從多個Uri取得絕對路徑。
         *
         * @param context     傳入Context
         * @param uris        傳入Uri陣列
         * @param mustCanRead 傳入Uri所指的路徑是否一定要可以讀取
         * @return 傳回絕對路徑字串陣列，若絕對路徑無法取得或是無法讀取，則對應的陣列索引位置為null
         */
        public static string[] getAbsolutePathsFromUris(Context context, Uri[] uris, bool mustCanRead)
        {
            if (uris == null)
            {
                return null;
            }
            int urisLength = uris.Length;
            string[] paths = new string[urisLength];
            for (int i = 0; i < urisLength; ++i)
            {
                Uri uri = uris[i];
                paths[i] = getAbsolutePathFromUri(context, uri, mustCanRead);
            }
            return paths;
        }

        /**
         * 從多個Uri取得File物件。
         *
         * @param context 傳入Context
         * @param uris    傳入Uri陣列
         * @return 傳回File物件陣列，若File物件無法建立，則對應的陣列索引位置為null
         */
        public static File[] getFilesFromUris(Context context, Uri[] uris)
        {
            return getFilesFromUris(context, uris, false);
        }

        /**
         * 從多個Uri取得File物件。
         *
         * @param context     傳入Context
         * @param uris        傳入Uri陣列
         * @param mustCanRead 傳入Uri所指的路徑是否一定要可以讀取
         * @return 傳回File物件陣列，若File物件無法建立或是檔案路徑無法讀取，則對應的陣列索引位置為null
         */
        public static File[] getFilesFromUris(Context context, Uri[] uris, bool mustCanRead)
        {
            if (uris == null)
            {
                return null;
            }
            int urisLength = uris.Length;
            File[] files = new File[urisLength];
            for (int i = 0; i < urisLength; ++i)
            {
                Uri uri = uris[i];
                files[i] = getFileFromUri(context, uri, mustCanRead);
            }
            return files;
        }

        /**
         * 從Uri取得絕對路徑。
         *
         * @param context 傳入Context
         * @param uri     傳入Uri物件
         * @return 傳回絕對路徑，若絕對路徑無法取得，傳回null
         */
        public static string getAbsolutePathFromUri(Context context, Uri uri)
        {
            return getAbsolutePathFromUri(context, uri, false);
        }

        /**
         * 從Uri取得絕對路徑。
         *
         * @param context     傳入Context
         * @param uri         傳入Uri物件
         * @param mustCanRead 傳入Uri所指的路徑是否一定要可以讀取
         * @return 傳回絕對路徑，若絕對路徑無法取得或是無法讀取，傳回null
         */
        public static string getAbsolutePathFromUri(Context context, Uri uri, bool mustCanRead)
        {
            File file = getFileFromUri(context, uri, mustCanRead);
            if (file != null)
            {
                return file.AbsolutePath;
            }
            else
            {
                return null;
            }
        }

        /**
         * 從Uri取得File物件。
         *
         * @param context 傳入Context
         * @param uri     傳入Uri物件
         * @return 傳回File物件，若File物件無法建立，傳回null
         */
        public static File getFileFromUri(Context context, Uri uri)
        {
            return getFileFromUri(context, uri, false);
        }

        /**
         * 從Uri取得File物件。
         *
         * @param context     傳入Context
         * @param uri         傳入Uri物件
         * @param mustCanRead 傳入Uri所指的路徑是否一定要可以讀取
         * @return 傳回File物件，若File物件無法建立或是檔案路徑無法讀取，傳回null
         */
        [SuppressLint(Value = new[] { "NewApi" })]
        public static File getFileFromUri(Context context, Uri uri, bool mustCanRead)
        {
            if (uri == null)
            {
                return null;
            }

            // 判斷是否為Android 4.4之後的版本
            bool after44 = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;
            if (after44 && DocumentsContract.IsDocumentUri(context, uri))
            {
                // 如果是Android 4.4之後的版本，而且屬於文件URI
                string authority = uri.Authority;
                // 判斷Authority是否為本地端檔案所使用的
                if (authority == "com.android.externalstorage.documents")
                {
                    // 外部儲存空間
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] divide = docId.Split(':');
                    string type = divide[0];
                    if (type == "primary")
                    {
                        string path = Environment.ExternalStorageDirectory.AbsolutePath + "/" + divide[1];
                        return createFileObjFromPath(path, mustCanRead);
                    }
                    else
                    {
                        string path = "/storage/" + type + "/" + divide[1];
                        return createFileObjFromPath(path, mustCanRead);
                    }
                }
                else if (authority == "com.android.providers.downloads.documents")
                {
                    // 下載目錄
                    string docId = DocumentsContract.GetDocumentId(uri);
                    Uri downloadUri = ContentUris.WithAppendedId(Uri.Parse("content://downloads/public_downloads"), Long.ParseLong(docId));
                    string path = queryAbsolutePath(context, downloadUri);
                    return createFileObjFromPath(path, mustCanRead);
                }
                else if (authority == "com.android.providers.media.documents")
                {
                    // 圖片、影音檔案
                    string docId = DocumentsContract.GetDocumentId(uri);
                    string[] divide = docId.Split(':');
                    string type = divide[0];
                    Uri mediaUri = null;
                    if (type == "image")
                    {
                        mediaUri = MediaStore.Images.Media.ExternalContentUri;
                    }
                    else if (type == "video")
                    {
                        mediaUri = MediaStore.Video.Media.ExternalContentUri;
                    }
                    else if (type == "audio")
                    {
                        mediaUri = MediaStore.Audio.Media.ExternalContentUri;
                    }
                    else
                    {
                        return null;
                    }
                    mediaUri = ContentUris.WithAppendedId(mediaUri, Long.ParseLong(divide[1]));
                    string path = queryAbsolutePath(context, mediaUri);
                    return createFileObjFromPath(path, mustCanRead);
                }
            }
            else
            {
                // 如果是一般的URI
                string scheme = uri.Scheme;
                string path = null;
                if (scheme == "content")
                {
                    // 內容URI
                    path = queryAbsolutePath(context, uri);
                }
                else if (scheme == "file")
                {
                    // 檔案URI
                    path = uri.Path;
                }
                return createFileObjFromPath(path, mustCanRead);
            }
            return null;
        }

        /**
         * 將路徑轉成File物件。
         *
         * @param path 傳入檔案路徑
         * @return 傳回File物件，若File物件無法建立，傳回null。
         */
        public static File createFileObjFromPath(string path)
        {
            return createFileObjFromPath(path, false);
        }

        /**
         * 將路徑轉成File物件。
         *
         * @param path        傳入檔案路徑
         * @param mustCanRead 傳入檔案路徑是否一定要可以讀取
         * @return 傳回File物件，若File物件無法建立或是檔案路徑無法讀取，傳回null
         */
        public static File createFileObjFromPath(string path, bool mustCanRead)
        {
            if (path != null)
            {
                try
                {
                    File file = new File(path);
                    if (mustCanRead)
                    {
                        file.SetReadable(true);
                        if (!file.CanRead())
                        {
                            return null;
                        }
                    }
                    return file.AbsoluteFile;
                }
                catch (Exception ex)
                {
                    ex.PrintStackTrace();
                }
            }
            return null;
        }

        /**
         * 查詢MediaStroe Uri對應的絕對路徑。
         *
         * @param context 傳入Context
         * @param uri     傳入MediaStore Uri
         * @return 傳回絕對路徑
         */
        public static string queryAbsolutePath(Context context, Uri uri)
        {
            string[] projection = { MediaStore.MediaColumns.Data };
            ICursor cursor = null;
            try
            {
                cursor = context.ContentResolver.Query(uri, projection, null, null, null);
                if (cursor != null && cursor.MoveToFirst())
                {
                    int index = cursor.GetColumnIndexOrThrow(MediaStore.MediaColumns.Data);
                    return cursor.GetString(index);
                }
            }
            catch (Exception ex)
            {
                ex.PrintStackTrace();
                if (cursor != null)
                {
                    cursor.Close();
                }
            }
            return null;
        }

        // TODO -----物件方法-----

        /**
         * 顯示檔案選取器，選取所有檔案，不設定檔案選取器的標題，僅進行單獨選取，被選到的檔案不一定要可以讀取。
         *
         * @return 傳回檔案選取器是否開啟成功
         */
        public bool showFileChooser()
        {
            return showFileChooser("*/*");
        }


        /**
         * 顯示檔案選取器，不設定檔案選取器的標題，僅進行單獨選取，被選到的檔案不一定要可以讀取。
         *
         * @param mimeType 傳入篩選的MIME類型
         * @return 傳回檔案選取器是否開啟成功
         */
        public bool showFileChooser(string mimeType)
        {
            return showFileChooser(mimeType, null);
        }

        /**
         * 顯示檔案選取器，僅進行單獨選取，被選到的檔案不一定要可以讀取。
         *
         * @param mimeType     傳入篩選的MIME類型
         * @param chooserTitle 傳入檔案選取器的標題，若為null則用預設值
         * @return 傳回檔案選取器是否開啟成功
         */
        public bool showFileChooser(string mimeType, string chooserTitle)
        {
            return showFileChooser(mimeType, chooserTitle, false);
        }


        /**
         * 顯示檔案選取器，被選到的檔案不一定要可以讀取。
         *
         * @param mimeType      傳入篩選的MIME類型
         * @param chooserTitle  傳入檔案選取器的標題，若為null則用預設值
         * @param allowMultiple 傳入檔案選取器是否使用複選
         * @return 傳回檔案選取器是否開啟成功
         */
        public bool showFileChooser(string mimeType, string chooserTitle, bool allowMultiple)
        {
            return showFileChooser(mimeType, chooserTitle, allowMultiple, false);
        }

        /**
         * 顯示檔案選取器。
         *
         * @param mimeType      傳入篩選的MIME類型
         * @param chooserTitle  傳入檔案選取器的標題，若為null則用預設值
         * @param allowMultiple 傳入檔案選取器是否使用複選
         * @param mustCanRead   傳入被選到的檔案是否一定要可以讀取
         * @return 傳回檔案選取器是否開啟成功
         */
        public bool showFileChooser(string mimeType, string chooserTitle, bool allowMultiple, bool mustCanRead)
        {
            if (mimeType == null || choosing)
            {
                return false;
            }
            choosing = true;
            // 檢查是否有可用的Activity
            PackageManager packageManager = activity.PackageManager;
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType(mimeType);
            IList<ResolveInfo> list = packageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            if (list.Count > 0)
            {
                this.mustCanRead = mustCanRead;
                // 如果有可用的Activity
                Intent picker = new Intent(Intent.ActionGetContent);
                picker.SetType(mimeType);
                picker.PutExtra(Intent.ExtraAllowMultiple, allowMultiple);
                picker.PutExtra(Intent.ExtraLocalOnly, true);
                // 使用Intent Chooser
                Intent destIntent = Intent.CreateChooser(picker, chooserTitle);
                if (fragment == null)
                {
                    activity.StartActivityForResult(destIntent, ACTIVITY_FILE_CHOOSER);
                }
                else
                {
                    fragment.StartActivityForResult(destIntent, ACTIVITY_FILE_CHOOSER);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /**
         * 當檔案選取器被關閉後，應該要呼叫這個方法，判斷檔案選取器是否有選取到檔案，接著再用getChosenFiles方法來取得選取結果。
         *
         * @param requestCode 傳入Activity的Request Code
         * @param resultCode  傳入Activity的Request Code
         * @param data        傳入Activity的data
         * @return 傳回檔案選取器是否有選取結果。
         */
        public bool onActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == ACTIVITY_FILE_CHOOSER)
            {
                choosing = false;
                if (resultCode == (int)Android.App.Result.Ok)
                {
                    oneUri = data.Data;
                    if (oneUri != null)
                    {
                        // 單選

                        chosenFiles = getFilesFromUris(activity, new Uri[] { oneUri }, mustCanRead);
                        return true;
                    }
                    else if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
                    {
                        // 複選
                        ClipData clipData = data.ClipData;
                        if (clipData != null)
                        {
                            int count = clipData.ItemCount;
                            if (count > 0)
                            {
                                Uri[] uris = new Uri[count];
                                for (int i = 0; i < count; ++i)
                                {
                                    uris[i] = clipData.GetItemAt(i).Uri;
                                }
                                chosenFiles = getFilesFromUris(activity, uris, mustCanRead);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /**
         * 取得被選取到的檔案。
         *
         * @return 傳回被選取到的檔案，過濾掉不成功的部份
         */
        public File[] getChosenFiles(bool filter)
        {
            if (chosenFiles == null)
            {
                return new File[0];
            }
            else
            {
                List<File> alFileList = new List<File>();
                foreach (File chosenFile in chosenFiles)
                {
                    if (filter && chosenFile == null)
                    {
                        continue;
                    }
                    alFileList.Add(chosenFile);
                }
                File[] files = new File[alFileList.Count];
                files = alFileList.ToArray();
                return files;
            }
        }

        /**
         * 取得被選取到的檔案。
         *
         * @return 傳回被選取到的檔案，過濾掉不成功的部份
         */
        public File[] getChosenFiles()
        {
            return getChosenFiles(true);
        }

    }
}