using Pingpp.Exception;
using Pingpp.Net;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Pingpp.Models;
using Pingpp.Utils;
using System.Runtime.InteropServices;

namespace Pingpp
{
    public abstract partial class Pingpp
    {
        protected HttpWebRequest GetRequest(string path, string method, string timestamp, string sign)
        {
            var request = (HttpWebRequest)WebRequest.Create(ApiBase + path);
            request.Headers.Add("Authorization", string.Format("Bearer {0}", ApiKey));
            if (!string.IsNullOrEmpty(ApiVersion))
            {
                request.Headers.Add("Pingplusplus-Version", ApiVersion);
            }

            request.Headers.Add("Pingplusplus-Request-Timestamp", timestamp);
            request.Headers.Add("Accept-Language", AcceptLanguage);
            if (!string.IsNullOrEmpty(sign))
            {
                request.Headers.Add("Pingplusplus-Signature", sign);
            }
            var userAgent = new Dictionary<string, object> {
                {"lang", "csharp"},
                {"publisher", "pingpp"},
                {"lang.version", Environment.Version.ToString()},
                {"os.version", Environment.OSVersion.ToString()},
                {"bindings.version", Version}
            };
            request.Headers.Add("Pingpp-Client-User-Agent", JsonConvert.SerializeObject(userAgent));

            request.UserAgent = "Pingpp/v1 CsharpBindings/" + Version;
            request.ContentType = "application/json;charset=utf-8";
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultReadAndWriteTimeout;
            request.Method = method;


            return request;
        }

        protected string DoRequest(string path, string method, Dictionary<string, object> param = null)
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new PingppException("No API key provided.  (HINT: set your API key using " +
                "\"Pingpp::setApiKey(<API-KEY>)\".  You can generate API keys from " +
                "the Pingpp web interface.  See https://pingxx.com/document/api for " +
                "details.");
            }
            try
            {
                HttpWebRequest req;
                HttpWebResponse res;
                method = method.ToUpper();
                string body = "", sign = "";
                string timestamp = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
                if ((method.Equals("POST") || method.Equals("PUT")) && param != null)
                {
                    body = JsonConvert.SerializeObject(param, Formatting.Indented);
                }

                // Sign the request
                try
                {
                    if (PrivateKey != null)
                    {
                        sign = RsaUtils.RsaSign(body + path + timestamp, PrivateKey);
                    }

                }
                catch (System.Exception e)
                {
                    throw new PingppException("Sign request error." + e.Message);
                }

                req = GetRequest(path, method, timestamp, sign);
                if (method.Equals("POST") || method.Equals("PUT"))
                {
                    using (var streamWriter = new StreamWriter(req.GetRequestStream()))
                    {
                        streamWriter.Write(body);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }
                }
                using (res = req.GetResponse() as HttpWebResponse)
                {
                    return res == null ? null : Requestor.ReadStream(res.GetResponseStream());
                }
            }
            catch (WebException e)
            {
                if (e.Response == null) throw new WebException(e.Message);
                var statusCode = ((HttpWebResponse)e.Response).StatusCode;
                if ((int)statusCode == 502 && BadGateWayMatch && MaxRetry < MaxNetworkRetries)
                {
                    MaxRetry++;
                    DoRequest(path, method, param);
                }
                var errors = Mapper<Error>.MapFromJson(Requestor.ReadStream(e.Response.GetResponseStream()), "error");

                throw new PingppException(errors, statusCode, errors.ErrorType, errors.Message);
            }
        }
    }
}
