using System.IO;
using hccPlayer;
using hccPlayer.iOS;
using Foundation;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Threading.Tasks;
using System;

[assembly: ExportRenderer (typeof(HybridWebView), typeof(HybridWebViewRenderer))]
namespace hccPlayer.iOS
{
	public class HybridWebViewRenderer : ViewRenderer<HybridWebView, WKWebView>, IWKScriptMessageHandler, IWKNavigationDelegate
    {
		const string JavaScriptFunction = "function invokeCSharpAction(cmd,data){window.webkit.messageHandlers.invokeAction.postMessage('{\"cmd\":\"' + cmd + '\", \"data\":\"' + data + '\"}');}";
        /*
         * https://stackoverflow.com/questions/9700904/overriding-xmlhttprequests-send-method
         * 
         !function(send){
            XMLHttpRequest.prototype.send = function (data) {
                send.call(this, data);
            }
        }(XMLHttpRequest.prototype.send);
         */
        WKUserContentController userController;
        WKWebView iOSWebView;
        protected override void OnElementChanged (ElementChangedEventArgs<HybridWebView> e)
		{
			base.OnElementChanged (e);

            if (Control == null) {
				userController = new WKUserContentController ();
				var script = new WKUserScript (new NSString (JavaScriptFunction), WKUserScriptInjectionTime.AtDocumentEnd, false);
				userController.AddUserScript (script);
				userController.AddScriptMessageHandler (this, "invokeAction");

				var config = new WKWebViewConfiguration { UserContentController = userController };
				iOSWebView = new WKWebView (Frame, config);
                iOSWebView.NavigationDelegate = this;

                SetNativeControl (iOSWebView);
			}
			if (e.OldElement != null) {
				userController.RemoveAllUserScripts ();
				userController.RemoveScriptMessageHandler ("invokeAction");
				var hybridWebView = e.OldElement as HybridWebView;
				hybridWebView.Cleanup ();
			}
			if (e.NewElement != null) {
				string fileName = Path.Combine (NSBundle.MainBundle.BundlePath, string.Format ("Content/{0}", Element.Uri));

                if (!string.IsNullOrEmpty(Element.Uri))
                {
                    if (Element.Uri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || Element.Uri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                    {
                        fileName = Element.Uri;
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName)));
                    }
                    else
                    {
                        fileName = Path.Combine(NSBundle.MainBundle.BundlePath, string.Format("Content/{0}", Element.Uri));
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(fileName, false)));
                    }
                }
                var webView = e.NewElement as HybridWebView;
                if (webView != null)
                {
                    webView.EvaluateJavascript = async (js) =>
                    {
                        NSObject nsObject = await Control.EvaluateJavaScriptAsync(js);

                        return (string)nsObject.ToString();
                    };
                    webView.Navigate = (url) =>
                    {
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(url)));
                        
                        return url;
                    };
                }
            }
		}
  

		public void DidReceiveScriptMessage (WKUserContentController userContentController, WKScriptMessage message)
		{
			Element.InvokeAction (message.Body.ToString ());
		}
        [Export("webView:didStartProvisionalNavigation:")]
        public void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            // When navigation starts, this gets called
            Console.WriteLine("DidStartProvisionalNavigation");
        }

        [Export("webView:didFinishNavigation:")]
        public void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Element.OnWebViewNavigationCompleted();
        }

    }
}
