using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpCachedServer
{
    public class HttpBaseServer
    {
        public string baseUrl { get; set; }
        public string url { get; set; }
        public int port { get; set; }
        /// <summary>
        /// returns the complete url with port and trailing /
        /// </summary>
        public string url_port { get { return this.url + ":" + this.port.ToString() + "/"; } }

        public string url_port_site { get { return this.url + ":" + this.port.ToString() + "/site/"; } }
        public string url_port_external { get { return this.url + ":" + this.port.ToString() + "/external/"; } }
        public string url_port_local { get { return this.url + ":" + this.port.ToString() + "/local/"; } }

        public HttpCachedServer.GetContentDelegate getContentDelegate { get; set; }

        public string BuildError(string message, string requestedUrl)
        {
            string errorHTML = "<HTML><BODY>";

            errorHTML += string.Format("requested page " + requestedUrl + " not found.<br>{0}", DateTime.Now);
            errorHTML += "Exception " + message;
            errorHTML += "</BODY></HTML>";
            return errorHTML;
        }
    }
}
