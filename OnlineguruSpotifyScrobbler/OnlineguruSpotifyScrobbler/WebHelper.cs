using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using OnlineguruSpotifyScrobbler.Properties;

namespace OnlineguruSpotifyScrobbler
{
    static class WebClientHelper
    {

        public static void DoAsyncRequest(string address, string data)
        {
            var payload = UTF8Encoding.UTF8.GetBytes(data);
            var client = new ExtendedWebClient();
            client.Timeout = 5000;
            client.Headers["Content-Type"] = "application/json; charset=utf-8";
            client.UploadDataCompleted += client_UploadDataCompleted;
            client.UploadDataAsync(new Uri(address, UriKind.Absolute), payload);
        }

        public static XElement DoSyncRequest(string address, string data)
        {
            var payload = UTF8Encoding.UTF8.GetBytes(data);
            var client = new ExtendedWebClient();
            client.Timeout = 5000;
            client.Headers["Content-Type"] = "application/json; charset=utf-8";
            var result = client.UploadData(new Uri(address, UriKind.Absolute), payload);
            return XElement.Parse(UTF8Encoding.UTF8.GetString(result));
        }

        private static void client_UploadDataCompleted(object sender, UploadDataCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.InnerException.ToString());
            }
            else
            {
                if (Settings.Default.Debug)
                {
                    MessageBox.Show("Return from webrequest:" + Environment.NewLine + UTF8Encoding.UTF8.GetString(e.Result));
                }
            }
        }
    }

    internal class ExtendedWebClient : WebClient
    {
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);
            if (request != null)
                request.Timeout = Timeout;
            return request;
        }

        public ExtendedWebClient()
        {
            Timeout = 10000; // the standard HTTP Request Timeout default
        }
    }
}
