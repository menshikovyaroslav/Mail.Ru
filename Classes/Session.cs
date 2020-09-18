using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mail.ru.Classes
{
    public static class Session
    {
        private static List<SessionCookie> _sessionCookies;

        private static Uri _uri = new Uri("https://mail.ru");

        private static string _user;
        private static string _password;
        private static string _proxy;

        static Session()
        {
            CheckCookieConfig();
            ReadAuthConfig();
        }

        public static void Check()
        {
            // If session isn't active we create a new session
            if (!IsActive())
            {
                Create();
            }


        }

        private static void CheckCookieConfig()
        {
            try
            {
                _sessionCookies = new List<SessionCookie>();

                // Path to saved cookies
                var pathToFile = AppDomain.CurrentDomain.BaseDirectory;
                var pathToSession = Path.Combine(pathToFile, "session");

                // Create session directory if not exists
                if (!Directory.Exists(pathToSession)) Directory.CreateDirectory(pathToSession);

                var pathToCookies = Path.Combine(pathToSession, "cookies.conf");

                // Read file for saved cookies and write it down into _sessionCookies
                var lines = File.ReadAllLines(pathToCookies);

                foreach (var line in lines)
                {
                    if (line.IsEmpty()) continue;
                    var splittedLine = line.Split(new char[] { '=' }, 2);
                    if (splittedLine.Length < 2) continue;

                    var sessionCookie = new SessionCookie(splittedLine[0], splittedLine[1]);
                    _sessionCookies.Add(sessionCookie);
                }
            }
            catch (Exception)
            {
            }
        }
        private static void ReadAuthConfig()
        {
            var pathToFile = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var pathToAuthConfig = Path.Combine(pathToFile, "auth.conf");

            var authFilePattern = "user=xxx\r\n" +
                                "password=yyy\r\n" +
                                "proxy=127.0.0.1:8888";

            if (!File.Exists(pathToAuthConfig)) File.AppendAllText(pathToAuthConfig, authFilePattern);

            var lines = File.ReadAllLines(pathToAuthConfig);

            foreach (var line in lines)
            {
                if (line.IsEmpty()) continue;
                var splittedLine = line.Split(new char[] { '=' }, 2);
                if (splittedLine.Length < 2) continue;

                switch (splittedLine[0])
                {
                    case "user":
                        _user = splittedLine[1];
                        break;
                    case "password":
                        _password = splittedLine[1];
                        break;
                    case "proxy":
                        _proxy = splittedLine[1];
                        break;
                }
            }
        } 

        private static bool IsActive()
        {
            try
            {
                // We keep cookies here
                var cookies = new CookieContainer();

                // Add our cookies to cookie container
                foreach (var cookie in _sessionCookies)
                {
                    cookies.Add(new Cookie(cookie.Name, cookie.Value) { Domain = _uri.Host });
                }

                // Check session
                var getRequest = new GetRequest()
                {
                    Address = $"https://portal.mail.ru/NaviData?mac=1&gamescnt=1&Socials=1&Login={_user}&rnd={DateTime.Now.ToUnixTime()}",
                    Accept = "*/*",
                    Host = "portal.mail.ru",
                    KeepAlive = true,
                    TimeOut = 10000,
                    Referer = "https://e.mail.ru/messages/inbox/?back=1&afterReload=1",
                    Proxy = new WebProxy(_proxy)
                };
                getRequest.Run(ref cookies);

                if (getRequest.Response.Contains("mail_cnt")) return true;

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void Create()
        {
            try
            {
                // We keep cookies here
                var cookies = new CookieContainer();

                // Any proxy, for example Fiddler
                var proxy = new WebProxy(_proxy);

                // we need to create this request to get 'windowName' parameter. it requires for auth
                var getRequest = new GetRequest()
                {
                    Address = "https://mail.ru/",
                    Accept = "text/html, application/xhtml+xml, image/jxr, */*",
                    Host = "mail.ru",
                    KeepAlive = true,
                    TimeOut = 10000,
                    Proxy = proxy
                };
                getRequest.Run(ref cookies);

                // find 'token' parameter
                var startIndex = getRequest.Response.IndexOf("CSRF:") + 6;
                var endIndex = getRequest.Response.IndexOf("\"", startIndex);
                var token = getRequest.Response.Substring(startIndex, endIndex - startIndex);

                // auth request
                var postRequest = new PostRequest()
                {
                    Data = $"login={WebUtility.UrlEncode(_user)}&password={WebUtility.UrlEncode(_password)}&saveauth=1&token={token}&project=e.mail.ru&_={DateTime.Now.ToUnixTime()}",
                    Address = @"https://auth.mail.ru/jsapi/auth",
                    Accept = "*/*",
                    Host = "auth.mail.ru",
                    ContentType = "application/x-www-form-urlencoded",
                    Referer = "https://mail.ru/",
                    KeepAlive = true,
                    Proxy = proxy
                };
                postRequest.Run(ref cookies);

                _sessionCookies = cookies.GetCookieCollection().CatalogCookies();
            }
            catch (Exception)
            {
            }
           
        }
    }
}
