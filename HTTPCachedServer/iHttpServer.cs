using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpCachedServer
{
    public delegate Byte[] GetContentDelegate(string url);

    public interface iHttpServer
    {
        void Run();
        void Stop();
        void ClearCache();
        string baseUrl { get; set; }
        string GetLocalIPAddress();
        string url { get; set; }
        int port { get; set; }
        string url_port { get; }
        string url_port_site { get; }
        string url_port_external { get; }
        string url_port_local { get; }

        Byte[] GetLocalContent(string url);
        GetContentDelegate getContentDelegate { get; set; }
    }
}
