using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;

namespace HTTPCachedServer.NET
{
    public class HTTPCachedServerNET : HttpCachedServer.HttpBaseServer,IDisposable, HttpCachedServer.iHttpServer
    {
        private WebServer webServer;


        public HTTPCachedServerNET()
        {
            string ipAddress = "localhost";
            this.url = "http://" + ipAddress;
            this.port = 8080;

            this.webServer = new WebServer(SendResponse, this.url + ":" + this.port);

        }
        public string SendResponse(HttpListenerRequest request)
        {
            Byte[] content = this.getContentDelegate?.Invoke(this.url_port + request.Url);


            return Encoding.UTF8.GetString(content);
        }

        public void Run()
        {
            webServer.Run();
        }

        public void Stop()
        {
            webServer.Stop();
        }
        public void Dispose()
        {
            this.webServer = null;
        }
        public Byte[] GetLocalContent(string requestedUrl)
        {
            return null;
        }
        public string GetLocalIPAddress()
        {
            return "";
        }
    }
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
                throw new NotSupportedException(
                "Needs Windows XP SP2, Server 2003 or later.");

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("prefixes");

            // A responder method is required
            if (method == null)
                throw new ArgumentException("method");

            foreach (string s in prefixes)
                _listener.Prefixes.Add(s);

            _responderMethod = method;
            _listener.Start();
        }

        public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
        : this(prefixes, method) { }

        public void Run()
        {
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver running...");
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                string rstr = _responderMethod(ctx.Request);
                                byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                            }
                            catch { } // suppress any exceptions
                                finally
                            {
                                    // always close the stream
                                    ctx.Response.OutputStream.Close();
                            }
                        }, _listener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
                });
        }

        public void Stop()
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}