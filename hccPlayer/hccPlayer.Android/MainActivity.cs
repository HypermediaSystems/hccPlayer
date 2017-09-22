using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using Java.Net;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using Android.Content.Res;
using System.Text;

namespace hccPlayer.Droid
{
    [Activity(Label = "hccPlayer", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        App thisApp;
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            thisApp = new App(new HMS.Net.Http.Android.SQLImplementation.SqlAndroid());
            LoadApplication(thisApp);
        }
        public override void OnDetachedFromWindow()
        {
            try
            {
                ((hccPlayer.HybridWebViewPage)thisApp.MainPage).stopServer();
                // ws?.Stop();
            }
            catch (Exception)
            {
            }
            base.OnDetachedFromWindow();
        }
        public string SendResponse(HttpListenerRequest request)
        {
            string url = request.Url.AbsolutePath.Substring(1);

            string content;
            try
            {
                AssetManager assets = this.Assets;
                using (StreamReader sr = new StreamReader(assets.Open("Content/" + url)))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                content = string.Format("<HTML><BODY>page " + request.Url + " not found.<br>{0}</BODY></HTML>", DateTime.Now);
            }

            return content;
        }
    }
}

