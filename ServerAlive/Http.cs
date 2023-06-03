using System.Net;
using System.Text;
using log4net;
using System.Threading;
using System;

namespace ServerAlive
{
    public class Http
    {
        static private readonly ILog _log = LogManager.GetLogger(typeof(Http));
        private HttpListener _listener;
        public void StartListen(string localAddress)
        {
            try
            {            
                _listener = new HttpListener();
                _listener.Prefixes.Add(localAddress);
                _listener.Start();
                _log.Info("Start listening...");
                while (true)
                {
                    HttpListenerContext context = _listener.GetContext();
                    HttpListenerResponse response = context.Response;
                    string responseText = "<html><head><meta charset='utf8'></head><body>ServerIsAlive!</body></html>";
                    byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    Thread.Sleep(1000);
                }
            }
            catch(Exception ex)
            {
                _listener.Close();
                _log.Error(ex);
            }
        }

        public void StopListen()
        {
            _listener.Close();
        }

        public bool CheckListener()
        {
            return _listener.IsListening;
        }

        public bool CheckConnection(string remoteAddress)
        {
            bool state = false;
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(remoteAddress);
            httpRequest.Timeout = 5000;
            httpRequest.Method = "GET";
            httpRequest.KeepAlive = true;
            using (HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse())
            {
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    state = true;
                }
                else
                {
                    state = false;
                }
                return state;
            }
        }
    }
}
