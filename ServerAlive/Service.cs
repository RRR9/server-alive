using System.Threading.Tasks;
using System.Configuration;
using System.Net;
using log4net;
using System.IO;

namespace ServerAlive
{
    static class Service
    {
        static private readonly ILog _log = LogManager.GetLogger(typeof(Service));
        static private string _localAddress;
        static private string _remoteAddress;
        static private bool _responseStatus;
        static private int _continuouslyNotResponse;

        static private Http _httpConnect;
        static public Http HttpConnect
        { 
            get
            {
                return _httpConnect;
            }
        }

        static public void Start()
        {
            _localAddress = ConfigurationManager.AppSettings["_localAddress"];
            _remoteAddress = ConfigurationManager.AppSettings["_remoteAddress"];
            _continuouslyNotResponse = 0;
            _httpConnect = new Http();
            bool SMSSended = false;

            while (true)
            {
                if(!_httpConnect.CheckListener())
                {
                    StartListenAsync(_httpConnect);
                }

                NetworkStatus.ShowNetworkInterfaces();
                NetworkStatus.CheckInternetConnection();
                _responseStatus = _httpConnect.CheckConnection(_remoteAddress) ? true : false;

                if (_responseStatus)
                {
                    _continuouslyNotResponse = 1;
                    SMSSended = false;
                }
                else
                {
                    _continuouslyNotResponse = (_continuouslyNotResponse + 1) % 21;
                }

                if (_continuouslyNotResponse >= 20 && SMSSended == false)
                {
                    SMSSended = true;
                    SendNotification("Server is not working", new string[] { "PhoneNumber1", "PhoneNumber2" }, "NameSender");
                }
                _log.Info($"Response from {_remoteAddress}...........: {_responseStatus}");
                _log.Info("");
                _log.Info("");
            }
        }

        static private void SendNotification(string message, string[] numbers, string sender)
        {
            _log.Info("  Sending Notification");
            string SMSAddress = ConfigurationManager.AppSettings["SMSCenter"];
            foreach (var number in numbers)
            {
                string url = SMSAddress + $"?message={message}&to={number}&from={sender}";
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Timeout = 10000;
                httpWebRequest.KeepAlive = true;
                using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    if (httpWebResponse.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream stream = httpWebResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                string response = streamReader.ReadToEnd();
                                _log.Info($"Response: {response}");
                            }
                        }
                        _log.Info("Message sending successful!");
                    }
                    else
                    {
                        _log.Info("Message sending failed!");
                    }
                }
            }
        }

        static private async void StartListenAsync(Http _httpConnect)
        {
            await Task.Run(() => _httpConnect.StartListen(_localAddress));
        }
    }
}
