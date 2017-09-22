using Android.Webkit;
using hccPlayer;
using hccPlayer.Droid;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Graphics;

[assembly: ExportRenderer (typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace hccPlayer.Droid
{
	public class HybridWebViewRenderer : ViewRenderer<HybridWebView, Android.Webkit.WebView>
	{
		const string JavaScriptFunction = "function invokeCSharpAction(cmd,data){JSBridge.invokeAction('{\"cmd\":\"' + cmd + '\", \"data\":\"' + data + '\"}');}";

        myWebViewClient mwvc;
        Android.Webkit.WebView androidWebView;
        protected override void OnElementChanged (ElementChangedEventArgs<HybridWebView> e)
		{
			base.OnElementChanged (e);
            
            if (Control == null) {
				androidWebView = new Android.Webkit.WebView (Forms.Context);                
                androidWebView.Settings.JavaScriptEnabled = true;
				SetNativeControl (androidWebView);
                mwvc = new myWebViewClient();
                androidWebView.SetWebViewClient(mwvc);

            }
			if (e.OldElement != null) {
				Control.RemoveJavascriptInterface ("JSBridge");
				var hybridWebView = e.OldElement as HybridWebView;
				hybridWebView.Cleanup ();
			}
			if (e.NewElement != null) {
				Control.AddJavascriptInterface (new JSBridge (this), "JSBridge");

                if (!string.IsNullOrEmpty(Element.Uri))
                {
                    if (Element.Uri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || Element.Uri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Control.LoadUrl(Element.Uri);
                    }
                    else
                    {
                        Control.LoadUrl(string.Format("file:///android_asset/Content/{0}", Element.Uri));
                    }
                }

                InjectJS (JavaScriptFunction);

                var webView = e.NewElement as HybridWebView;
                if (webView != null)
                {
                    webView.EvaluateJavascript = async (js) =>
                    {
                        var reset = new ManualResetEvent(false);
                        var response = string.Empty;
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Control?.EvaluateJavascript(js, new JavascriptCallback((r) => { response = r; reset.Set(); }));
                        });
                        await Task.Run(() => { reset.WaitOne(); });
                        return response;
                    };
                    webView.Navigate = (url) =>
                    {
                        System.Diagnostics.Debug.WriteLine("webView.Navigate START: " + url);
                        Control.LoadUrl(url);

                        // string data = @"<html><head><script type='text/javascript'></script></head><body><a href='https://www.bing.de'>new Url</a></body></html>";
                        // Control.LoadDataWithBaseURL(url, data, "text/html", "UTF-8", null);
                        System.Diagnostics.Debug.WriteLine("webView.Navigate END: " + url);

                        return url;
                    };

                    mwvc.OnWebViewNavigationCompleted = () => {
                        webView.OnWebViewNavigationCompleted();
                    };
                }

            }
        }
        internal class JavascriptCallback : Java.Lang.Object, IValueCallback
        {
            public JavascriptCallback(Action<string> callback)
            {
                _callback = callback;
            }

            private System.Action<string> _callback;
            public void OnReceiveValue(Java.Lang.Object value)
            {
                _callback?.Invoke(Convert.ToString(value));
            }
        }

        void InjectJS (string script)
		{
			if (Control != null) {
				Control.LoadUrl (string.Format ("javascript: {0}", script));
			}
		}

    }
    class myWebViewClient: WebViewClient
    {
        public delegate void OnPageFinishedDelegate();
        public OnPageFinishedDelegate OnWebViewNavigationCompleted;
        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            return false;
            // return base.ShouldOverrideUrlLoading(view, request);
        }
        [Obsolete]
        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine("ShouldOverrideUrlLoading: " + url);
            view.LoadUrl(url);
            return true;
            // return base.ShouldOverrideUrlLoading(view, url);
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            OnWebViewNavigationCompleted?.Invoke();
            System.Diagnostics.Debug.WriteLine("OnPageFinished: " + url);
            base.OnPageFinished(view, url);
        }
        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            System.Diagnostics.Debug.WriteLine("OnPageStarted: " + url);
            base.OnPageStarted(view, url, favicon);
        }
        [Obsolete]
        public override WebResourceResponse ShouldInterceptRequest(Android.Webkit.WebView view, string url)
        {
            System.Diagnostics.Debug.WriteLine("ShouldInterceptRequest: " + url);
            return base.ShouldInterceptRequest(view, url);
        }
        public override WebResourceResponse ShouldInterceptRequest(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            System.Diagnostics.Debug.WriteLine("ShouldInterceptRequest2: " + request.Url);
            return base.ShouldInterceptRequest(view, request);
        }
    }
}
