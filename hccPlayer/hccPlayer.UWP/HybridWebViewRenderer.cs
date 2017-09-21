using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hccPlayer;
using hccPlayer.UWP;
using Xamarin.Forms.Platform.UWP;
using Windows.UI.Xaml.Controls;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel;
using UWP.Notify;

[assembly:ExportRenderer(typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace hccPlayer.UWP
{
    public class HybridWebViewRenderer : ViewRenderer<HybridWebView, Windows.UI.Xaml.Controls.WebView>
    {
        const string JavaScriptFunction = "function invokeCSharpAction(cmd,data){JSBridge.notify('{\"cmd\":\"' + cmd + '\", \"data\":\"' + data + '\"}');}";

        Windows.UI.Xaml.Controls.WebView webView;
        // see https://www.suchan.cz/2016/01/hacking-uwp-webview-part-2-bypassing-window-external-notify-whitelist/
        private sharedObj communicationWinRT = new sharedObj();
        protected override void OnElementChanged(ElementChangedEventArgs<HybridWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                webView = new Windows.UI.Xaml.Controls.WebView();
                SetNativeControl(webView);
            }
            if (e.OldElement != null)
            {
                Control.NavigationStarting -= Control_NavigationStarting;
                Control.ScriptNotify -= OnWebViewScriptNotify;
            }
            if (e.NewElement != null)
            {
                // Control.NavigationCompleted += OnWebViewNavigationCompleted;
                Control.NavigationStarting += Control_NavigationStarting;
                Control.ScriptNotify += OnWebViewScriptNotify;
                
                setSource(Element.Uri);

                var webView = e.NewElement as HybridWebView;
                if (webView != null)
                {
                    webView.EvaluateJavascript = async (js) =>
                    {
                        return await Control.InvokeScriptAsync("eval", new[] { js });
                    };
                    
                    webView.Navigate = (url) =>
                    {
                        setSource(url);
                        return url;
                    };

                    Control.NavigationCompleted += async (WebView sender, WebViewNavigationCompletedEventArgs args) => {
                        if (args.IsSuccess)
                        {
                            // Inject JS script
                            await Control.InvokeScriptAsync("eval", new[] { JavaScriptFunction });
                        }
                        webView.OnWebViewNavigationCompleted();
                    };

                    communicationWinRT.notifyHandler = (string msg) => {
                        Element.InvokeAction(msg);
                    };
                }


            }
        }

        private void Control_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            webView.AddWebAllowedObject("JSBridge", communicationWinRT);

        }

        private void setSource(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
            {
                if (uri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || uri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                {
                    Control.Source = new Uri(uri);
                }
                else
                {
                    Control.Source = new Uri(string.Format("ms-appx-web:///Content//{0}", uri));
                }
            }
        }
        async void OnWebViewNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            
            
        }

        void OnWebViewScriptNotify(object sender, NotifyEventArgs e)
        {
            
            Element.InvokeAction(e.Value);
        }

        
    }
   

}
