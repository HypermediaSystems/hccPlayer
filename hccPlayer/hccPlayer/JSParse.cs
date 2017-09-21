using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace hccPlayer
{
    class JSParse
    {
        private Jint.Engine engine;
        private string command;
        private HWM hwm;
        public JSParse(HybridWebView hybridWebView, HWM hwm, string command)
        {
            HWM.hybridWebView = hybridWebView;
            this.hwm = hwm;
            this.command = command;
            this.engine = new Jint.Engine( (options) => { options.DebugMode(); });
            this.engine.Step += (sender,info) =>{
                Device.BeginInvokeOnMainThread(() => {
                    System.Diagnostics.Debug.WriteLine(info.CurrentStatement);
                });

                return Jint.Runtime.Debugger.StepMode.Into;
            };
            

            this.engine.SetValue("HWM", hwm);
            this.engine.SetValue("print", new Action<object>(Print));
            // add all function from HWM

            Type myType = (typeof(HWM));

            List<MethodInfo> methodInfos = myType.GetRuntimeMethods().ToList<MethodInfo>();
            foreach(var m in methodInfos)
            {
                System.Diagnostics.Debug.WriteLine(m.Name);
            }
            // here we add some entry functions with out the HWM namespace
            this.engine.SetValue("JQuery", new Func<string, HWM>(hwm.JQuery));
            this.engine.SetValue("$", new Func<string, HWM>(hwm.JQuery));
            this.engine.SetValue("navigate", new Func<string, Func<Jint.Native.JsValue>, string>(hwm.navigate));
            this.engine.SetValue("wait", new Func<string, Func<Jint.Native.JsValue>, string>(hwm.wait));


        }
        public void execute()
        {
            try
            {
                this.engine.Execute(command);
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    System.Diagnostics.Debug.WriteLine("JSParse Error: " + ex.Message);
                    this.hwm.trace?.Invoke("JSParse Error: " + ex.Message);
                });
            }
        }
        public void Print(object s)
        {
            if (s == null)
                s = "null";
            Device.BeginInvokeOnMainThread(() =>
            {
                System.Diagnostics.Debug.WriteLine(s);
                this.hwm.trace?.Invoke((string)s);

            });
        }
    }
}
