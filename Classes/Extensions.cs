using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mail.ru.Classes
{
    public static class Extensions
    {
        public static bool IsEmpty(this string x)
        {
            return string.IsNullOrEmpty(x);
        }

        public static int ToUnixTime(this DateTime date)
        {
            int unixTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            return unixTime;
        }

        public static List<SessionCookie> CatalogCookies(this CookieCollection collection)
        {
            return (from Cookie cookie in collection select new SessionCookie(cookie.Name, cookie.Value)).ToList();
        }

        public static CookieCollection GetCookieCollection(this CookieContainer cookieJar)
        {

            CookieCollection cookieCollection = new CookieCollection();

            if (cookieJar == null) return cookieCollection;

            Hashtable table = (Hashtable)cookieJar.GetType().InvokeMember("m_domainTable",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            cookieJar,
                                                                            new object[] { });

            foreach (var tableKey in table.Keys)
            {
                String str_tableKey = (string)tableKey;

                if (str_tableKey[0] == '.')
                {
                    str_tableKey = str_tableKey.Substring(1);
                }

                SortedList list = (SortedList)table[tableKey].GetType().InvokeMember("m_list",
                                                                            BindingFlags.NonPublic |
                                                                            BindingFlags.GetField |
                                                                            BindingFlags.Instance,
                                                                            null,
                                                                            table[tableKey],
                                                                            new object[] { });

                foreach (var listKey in list.Keys)
                {
                    String url = "https://" + str_tableKey + (string)listKey;
                    cookieCollection.Add(cookieJar.GetCookies(new Uri(url)));
                }
            }

            return cookieCollection;
        }
    }
}
