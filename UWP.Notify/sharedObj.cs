using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;

namespace UWP.Notify
{
    public delegate void NotifyHandler(string msg);
    [AllowForWeb]
    public sealed class sharedObj
    {
        public NotifyHandler notifyHandler { get; set; }
        public void notify(string msg)
        {
            // do something else
            notifyHandler?.Invoke(msg);
        }
        public string getSrc(string oldSrc)
        {
            return "new src from " + oldSrc;
        }

    }
}
