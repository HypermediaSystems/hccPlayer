using System;
using Android.Webkit;
using hccPlayer.Droid;
using Java.Interop;

namespace hccPlayer.Droid
{
	public class JSBridge : Java.Lang.Object
	{
		readonly WeakReference<HybridWebViewRenderer> hybridWebViewRenderer;

		public JSBridge (HybridWebViewRenderer hybridRenderer)
		{
			hybridWebViewRenderer = new WeakReference <HybridWebViewRenderer> (hybridRenderer);
		}

		[JavascriptInterface]
		[Export ("invokeAction")]
		public void InvokeAction (string data)
		{
			HybridWebViewRenderer hybridRenderer;

			if (hybridWebViewRenderer != null && hybridWebViewRenderer.TryGetTarget (out hybridRenderer)) {
				hybridRenderer.Element.InvokeAction (data);
			}
            
		}
        [JavascriptInterface]
        [Export("getSrc")]
        public string getSrc(string oldSrc)
        {
            return "new src from " + oldSrc;
        }
	}
}

