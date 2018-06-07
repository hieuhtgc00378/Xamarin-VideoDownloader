using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Threading.Tasks;
using Android;
using VideoDownloader.Droid;
using Android.Media;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(MainActivity))]
namespace VideoDownloader.Droid
{
    [Activity(Label = "VideoDownloader", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IDownloadState
    {
        readonly string[] StoragePermissions =
        {
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.ReadExternalStorage
        };

        const int RequestStorageId = 0;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
            int sdkBuildVersion = (int)Build.VERSION.SdkInt;
            Console.WriteLine("SDK build version = " + sdkBuildVersion);
            if (sdkBuildVersion >= 23)
            {
                RequestStoragePermission();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestStorageId:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            Console.WriteLine("permission - " + permissions[0] + " has been granted");
                        }
                        else
                        {
                            Console.WriteLine("permission - " + permissions[0] + " has not been granted");
                        }
                    }
                    break;
                default:
                    break;
            }
        }


        async Task RequestStoragePermission()
        {
            //Check to see if any permission in our group is available, if one, then all are
            const string writePermission = Manifest.Permission.WriteExternalStorage;
            const string readPermission = Manifest.Permission.ReadExternalStorage;
            int permited = 0;
            if (CheckSelfPermission(writePermission) == (int)Permission.Granted)
            {
                Console.WriteLine("writePermission permited");
            }
            else
            {
                permited = -1;
            }

            if (CheckSelfPermission(readPermission) == (int)Permission.Granted)
            {
                Console.WriteLine("readPermission permited");
            }
            else
            {
                permited = -1;
            }
            if (permited == -1)
            {
                RequestPermissions(StoragePermissions, RequestStorageId);
            }
        }

        public string OnDownloadStarted()
        {
            return string.Empty;
        }

        public void OnDownloadError()
        {
            throw new NotImplementedException();
        }

        public void OnDownloadFinished(string path)
        {
            try
            {
                //Intent intent = new Intent(Intent.ActionMediaMounted);
                //SendBroadcast(intent);
                //throw new NotImplementedException();
                try
                {
                    Looper.Prepare();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception OnDownloadFinished, ex = " + ex.ToString());
                }
                MediaScannerConnection.ScanFile(Forms.Context, new String[] { path }, null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception OnDownloadFinished, ex = " + ex.ToString());
            }
        }

        public void SendBroadcastToGallery()
        {
            //if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Kitkat)
            //{
            //    Intent scanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            //    Uri contentUri = Uri.fromFile(outputFile);
            //    scanIntent.setData(contentUri);
            //    sendBroadcast(scanIntent);
            //}
            //else
            //{
            //    final Intent intent = new Intent(Intent.ACTION_MEDIA_MOUNTED, Uri.parse("file://" + Environment.getExternalStorageDirectory()));
            //    sendBroadcast(intent);
            //}
        }
    }
}

