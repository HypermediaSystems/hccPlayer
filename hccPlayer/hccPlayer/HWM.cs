using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccPlayer
{
    class HWM
    {
        JQuery jQuery;
        public static HybridWebView hybridWebView;
        public delegate void traceDelegate(string str);
        public traceDelegate trace;

        public HWM JQuery(string selector)
        {            
            this.jQuery = new JQuery(HWM.hybridWebView, selector);
            return this;
        }
        public string css(string cssName, Func<Jint.Native.JsValue, Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.css(cssName);
                callBackFunction?.Invoke(jArray.ToString());
            });
            return ret;
        }
        public HWM css(string cssName, string cssValue)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.css(cssName,cssValue);                
            });
            return this;
        }

        public string attr(string attrName, Func<Jint.Native.JsValue, Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.attr(attrName);
                callBackFunction?.Invoke(jArray.ToString());
            });
            return ret;
        }
        public HWM attr(string attrName, string attrValue)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.attr(attrName, attrValue);
            });
            return this;
        }
        public string text( Func<Jint.Native.JsValue, Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.text();
                callBackFunction?.Invoke(jArray.ToString());
            });
            return ret;
        }
        public HWM text(string newtext)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.text(newtext);
            });
            return this;
        }

        public string wait(string arg, Func<Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            Device.BeginInvokeOnMainThread(async () =>
            {
                if (arg == "???")
                {
                    //   var answer = await DisplayAlert("Question?", "Would you like to play a game", "Yes", "No");
                    callBackFunction();
                }
                else
                {
                    int msec = 500;
                    int.TryParse(arg, out msec);
                    await Task.Delay(msec);
                    callBackFunction?.Invoke();
                }
            });
            return ret;
        }
        public string navigate(string newUrl, Func<Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            hybridWebView.Navigating = true;

            Device.BeginInvokeOnMainThread(async () =>
            {
                hybridWebView.Navigate(newUrl);
                
                // ToDo add timeout
                while (hybridWebView.Navigating == true)
                {
                    System.Diagnostics.Debug.WriteLine("navigating ...");
                    await Task.Delay(1000);
                }
                callBackFunction?.Invoke();
                System.Diagnostics.Debug.WriteLine("navigated");
            });

            System.Diagnostics.Debug.WriteLine("navigate() return;");
            return ret;
        }
        public string enumerate(string[] enumList, Func<Jint.Native.JsValue, Jint.Native.JsValue> callBackFunction)
        {
            string ret = "";
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.enumerate(String.Join(",",enumList));
                callBackFunction?.Invoke(jArray.ToString());
            });
            return ret;
        }
        
        public void click()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.click();
            });
        }
        public void submit()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                JArray jArray = await this.jQuery.submit();
            });
        }
    }
}
