using System;

using Xamarin.Forms;

namespace hccPlayer
{
	public class HybridWebViewPageCSx : ContentPage
	{
		public HybridWebViewPageCSx ()
		{
			var hybridWebView = new HybridWebView {
                // Uri = "http://www.google.de", // index.html",
                Uri = "index.html",
                HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand
			};

			hybridWebView.RegisterAction (data => DisplayAlert ("Alert", "Hello " + data, "OK"));

			Padding = new Thickness (0, 20, 0, 0);
			Content = hybridWebView;
		}
	}
}
