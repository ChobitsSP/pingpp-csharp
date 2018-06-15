using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Pingpp.Exception;
using Pingpp.Models;
using Pingpp.Utils;
using System.Runtime.InteropServices;

namespace Pingpp.Net
{
    internal static class Requestor
    {
        internal static string ReadStream(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        internal static Dictionary<string, string> FormatParams(Dictionary<string, object> param)
        {
            if (param == null)
            {
                return new Dictionary<string, string>();
            }
            var formattedParam = new Dictionary<string, string>();
            foreach (var dic in param)
            {
                var dicts = dic.Value as Dictionary<string, string>;
                if (dicts != null)
                {
                    var formatNestedDic = new Dictionary<string, object>();
                    foreach (var nestedDict in dicts)
                    {
                        formatNestedDic.Add(string.Format("{0}[{1}]", dic.Key, nestedDict.Key), nestedDict.Value);
                    }

                    foreach (var nestedDict in FormatParams(formatNestedDic))
                    {
                        formattedParam.Add(nestedDict.Key, nestedDict.Value);
                    }
                }
                else if (dic.Value is Dictionary<string, object>)
                {
                    var formatNestedDic = new Dictionary<string, object>();

                    foreach (var nestedDict in (Dictionary<string, object>)dic.Value)
                    {
                        formatNestedDic.Add(string.Format("{0}[{1}]", dic.Key, nestedDict.Key), nestedDict.Value.ToString());
                    }

                    foreach (var nestedDict in FormatParams(formatNestedDic))
                    {
                        formattedParam.Add(nestedDict.Key, nestedDict.Value);
                    }
                }
                else if (dic.Value is IList)
                {
                    var li = (List<object>)dic.Value;
                    var formatNestedDic = new Dictionary<string, object>();
                    var size = li.Count();
                    for (var i = 0; i < size; i++)
                    {
                        formatNestedDic.Add(string.Format("{0}[{1}]", dic.Key, i), li[i]);
                    }
                    foreach (var nestedDict in FormatParams(formatNestedDic))
                    {
                        formattedParam.Add(nestedDict.Key, nestedDict.Value);
                    }
                }
                else if ("".Equals(dic.Value))
                {
                    throw new PingppException(string.Format("You cannot set '{0}' to an empty string. " +
                        "We interpret empty strings as null in requests. " +
                        "You may set '{0}' to null to delete the property.", dic.Key));
                }
                else if (dic.Value == null)
                {
                    formattedParam.Add(dic.Key, "");
                }
                else
                {
                    formattedParam.Add(dic.Key, dic.Value.ToString());
                }

            }
            return formattedParam;
        }

        internal static string CreateQuery(Dictionary<string, object> param)
        {
            var flatParams = FormatParams(param);
            var queryStringBuffer = new StringBuilder();
            foreach (var entry in flatParams)
            {
                if (queryStringBuffer.Length > 0)
                {
                    queryStringBuffer.Append("&");
                }

                queryStringBuffer.Append(UrlEncodePair(entry.Key, entry.Value));
            }
            return queryStringBuffer.ToString();
        }

        internal static string UrlEncodePair(string k, string v)
        {
            return string.Format("{0}={1}", UrlEncode(k), UrlEncode(v));
        }

        internal static string UrlEncode(string str)
        {
            return string.IsNullOrEmpty(str) ? null : HttpUtility.UrlEncode(str, Encoding.UTF8);
        }

        internal static string FormatUrl(string url, string query)
        {
            return string.IsNullOrEmpty(query) ? url : string.Format("{0}{1}{2}", url, url.Contains("?") ? "&" : "?", query);
        }
    }
}