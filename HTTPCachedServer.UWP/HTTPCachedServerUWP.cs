using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;

using HttpCachedServer.UWP.Implementation;
[assembly: Xamarin.Forms.Dependency(typeof(HTTPCachedServerUWP))]
namespace HttpCachedServer.UWP.Implementation
{
    public class HTTPCachedServerUWP : HttpCachedServer.HttpBaseServer, IDisposable, HttpCachedServer.iHttpServer
    {
        private const uint BufferSize = 8192;
        private static readonly StorageFolder LocalFolder
                     = Windows.ApplicationModel.Package.Current.InstalledLocation;

        private StreamSocketListener listener;

        public HTTPCachedServerUWP()
        {
            string ipAddress = "localhost";
            this.url = "http://" + ipAddress;
            this.port = 8080;
        }

        public async void Run()
        {
            this.Stop();
            this.listener = new StreamSocketListener();
            this.listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            await this.listener.BindServiceNameAsync(this.port.ToString());
        }

        public void Stop()
        {
            this.listener?.Dispose();
        }
        public void ClearCache()
        {
            Task.Run(async () =>
            {
                await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
            }).ConfigureAwait(false);
        }
        public string GetLocalIPAddress()
        {
            List<string> ipAddresses = new List<string>();
            var hostnames = NetworkInformation.GetHostNames();
            foreach (var hn in hostnames)
            {
                //IanaInterfaceType == 71 => Wifi
                //IanaInterfaceType == 6 => Ethernet (Emulator)
                if (hn.IPInformation != null &&
                    (hn.IPInformation.NetworkAdapter.IanaInterfaceType == 71
                    || hn.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                {
                    string ipAddress = hn.DisplayName;
                    ipAddresses.Add(ipAddress);
                }
            }

            if (ipAddresses.Count < 1)
            {
                return null;
            }
            else if (ipAddresses.Count == 1)
            {
                return ipAddresses[0];
            }
            else
            {
                return ipAddresses[ipAddresses.Count - 1];
            }
        }

        public void Dispose()
        {
            this.listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            // this works for text only
            StringBuilder request = new StringBuilder();
            using (IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[BufferSize];
                IBuffer buffer = data.AsBuffer();
                uint dataRead = BufferSize;
                while (dataRead == BufferSize)
                {
                    await input.ReadAsync(buffer, BufferSize, InputStreamOptions.Partial);
                    request.Append(Encoding.UTF8.GetString(data, 0, data.Length));
                    dataRead = buffer.Length;
                }
            }

            using (IOutputStream output = socket.OutputStream)
            {
                string requestMethod = request.ToString().Split('\n')[0];
                string[] requestParts = requestMethod.Split(' ');

                if (requestParts[0] == "GET")
                {
                    await WriteResponseAsync(requestParts[1], output);
                }
                else
                {
                    throw new InvalidDataException("HTTP method not supported: "
                                 + requestParts[0]);
                }
            }
        }

        private async Task WriteResponseAsync(string path, IOutputStream os)
        {
            using (Stream resp = os.AsStreamForWrite())
            {
                bool exists = true;
                try
                {
                    Byte[] content = this.getContentDelegate?.Invoke(this.url_port + path.Substring(1));
                    if (content == null || content.Length == 0)
                        content = this.GetLocalContent(path);

                    string header = String.Format("HTTP/1.1 200 OK\r\n" +
                                        "Content-Length: {0}\r\n" +
                                        "Connection: close\r\n\r\n",
                                        content.Length);
                    byte[] headerArray = Encoding.UTF8.GetBytes(header);
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);

                    await resp.WriteAsync(content, 0, content.Length);

                }
                catch (FileNotFoundException)
                {
                    exists = false;
                }

                if (!exists)
                {
                    byte[] headerArray = Encoding.UTF8.GetBytes(
                                          "HTTP/1.1 404 Not Found\r\n" +
                                          "Content-Length:0\r\n" +
                                          "Connection: close\r\n\r\n");
                    await resp.WriteAsync(headerArray, 0, headerArray.Length);
                }

                await resp.FlushAsync();
            }
        }
        public Byte[] GetLocalContent(string requestedUrl)
        {
            while (requestedUrl.StartsWith("/") || requestedUrl.StartsWith("\\"))
                requestedUrl = requestedUrl.Substring(1);

            Byte[] content = Encoding.UTF8.GetBytes(this.BuildError("Url not found", requestedUrl));
            try
            {
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                string filePath = "Content\\" + requestedUrl.Replace('/', '\\');
                Task<Stream> fsTask = InstallationFolder.OpenStreamForReadAsync(filePath);
                Task continuation = fsTask.ContinueWith(t =>
                {
                    if (t.Status == TaskStatus.RanToCompletion)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            t.Result.CopyTo(ms);
                            content = ms.ToArray();
                        }
                    }
                    else
                    {
                        // ToDo log the exception
                        content = Encoding.UTF8.GetBytes(this.BuildError("Error: ", requestedUrl));
                    }
                });
                continuation.Wait();
            }
            catch (Exception ex)
            {
                // ToDo log the exception
                content = Encoding.UTF8.GetBytes(this.BuildError(ex.Message, requestedUrl));
            }

            return content;
        }
    }
}
