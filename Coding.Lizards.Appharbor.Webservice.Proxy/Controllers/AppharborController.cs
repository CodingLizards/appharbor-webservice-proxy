using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using AppHarbor;
using Coding.Lizards.Appharbor.Webservice.Proxy.Models;
using RestSharp.Contrib;

namespace Coding.Lizards.Appharbor.Webservice.Proxy.Controllers {

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AppharborController : ApiController {

        [HttpGet]
        public string Authenticate(string clientId, string redirectUrl) {
#if DEBUG
            var redUrl = "http://" + Request.RequestUri.Host + ":" + Request.RequestUri.Port + "/appharbor/authenticated";
#else
            var redUrl = "https://appharbor-webservice-proxy.apphb.com/appharbor/authenticated";
#endif
            var url = string.Format("https://appharbor.com/user/authorizations/new?client_id={0}&redirect_uri={1}&state={2}", clientId, HttpUtility.UrlEncode(redUrl), HttpUtility.UrlEncode(redirectUrl));
            return url;
        }

        [HttpGet]
        public RedirectResult Authenticated(string code, string state) {
            if (state.Contains("?")) {
                state += "&code=" + code;
            } else {
                state += "?code=" + code;
            }
            return Redirect(state);
        }

        //public JsonResult<dynamic> GetRequest(string url, string access_token) {
        //    var req = HttpWebRequest.CreateHttp(url);
        //    req.Headers.Add(HttpRequestHeader.Authorization, string.Format("BEARER {0}", access_token));

        //}
    }
}
