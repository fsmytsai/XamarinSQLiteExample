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
using Android.Support.V4.Util;

namespace FrogCroak.MyMethod
{
    public class MyLruCache : LruCache
    {
        public MyLruCache(int size) : base(size) { }

        protected override int SizeOf(Java.Lang.Object key, Java.Lang.Object value)
        {
            // android.graphics.Bitmap.getByteCount() method isn't currently implemented in Xamarin. Invoke Java method.
            IntPtr classRef = JNIEnv.FindClass("android/graphics/Bitmap");
            var getBytesMethodHandle = JNIEnv.GetMethodID(classRef, "getByteCount", "()I");
            var byteCount = JNIEnv.CallIntMethod(value.Handle, getBytesMethodHandle);

            return byteCount / 1024;
        }
    }
}