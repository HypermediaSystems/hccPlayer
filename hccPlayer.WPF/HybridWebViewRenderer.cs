using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using hccPlayer;
using hccPlayer.WPF;

using System.Windows.Controls;

namespace hccPlayer.WPF
{
    public class HybridWebViewRenderer 
    {
        const string JavaScriptFunction = "function invokeCSharpAction(cmd,data){JSBridge.notify('{\"cmd\":\"' + cmd + '\", \"data\":\"' + data + '\"}');}";
        readonly WebBrowser webView;
        
        public HybridWebViewRenderer()
        {
            this.webView = new WebBrowser();
        }


    }


}

