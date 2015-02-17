using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;
using AppHarbor;
using Coding.Lizards.Appharbor.Webservice.Proxy.Models;
using Newtonsoft.Json;
using RestSharp.Contrib;

namespace Coding.Lizards.Appharbor.Webservice.Proxy.Controllers {

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AppharborController : ApiController {

        [HttpGet]
        public string Authenticate(string clientId, string clientSecret, string redirectUrl) {
#if DEBUG
            var redUrl = "http://" + Request.RequestUri.Host + ":" + Request.RequestUri.Port + "/appharbor/authenticated";
#else
            var redUrl = "https://appharbor-webservice-proxy.apphb.com/appharbor/authenticated";
#endif
            var uriBuilder = new UriBuilder(redirectUrl);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["clientSecret"] = clientSecret;
            query["clientId"] = clientId;
            var items = new List<string>();
            foreach (var item in query.AllKeys) {
                items.Add(item + "=" + query[item]);
            }
            uriBuilder.Query = string.Join("&", items);
            var url = string.Format("https://appharbor.com/user/authorizations/new?client_id={0}&redirect_uri={1}&state={2}", clientId, HttpUtility.UrlEncode(redUrl), HttpUtility.UrlEncode(uriBuilder.ToString()));
            return url;
        }

        [HttpGet]
        public async Task<RedirectResult> Authenticated(string code, string state) {
            var uriBuilder = new UriBuilder(state);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            var atReq = HttpWebRequest.CreateHttp(string.Format("https://appharbor.com/tokens"));
            atReq.Method = HttpMethod.Post.Method;
            var postContent = Encoding.ASCII.GetBytes(string.Format("client_id={0}&client_secret={1}&code={2}", query["clientId"], query["clientSecret"], code));
            atReq.ContentLength = postContent.Length;
            atReq.ContentType = "application/x-www-form-urlencoded";
            using (var requestStream = await atReq.GetRequestStreamAsync()) {
                await requestStream.WriteAsync(postContent, 0, postContent.Length);
            }
            using (var resp = await atReq.GetResponseAsync()) {
                using (var sr = new StreamReader(resp.GetResponseStream())) {
                    var tokenData = await sr.ReadToEndAsync();
                    var accessTokenBuilder = new UriBuilder(state);
                    accessTokenBuilder.Query = tokenData;
                    return Redirect(accessTokenBuilder.ToString());
                }
            }
        }

        [HttpGet]
        public async Task<dynamic> GetRequest(string url, string accesstoken) {
            try {
                var req = HttpWebRequest.CreateHttp(url);
                req.Headers.Add(HttpRequestHeader.Authorization, string.Format("BEARER {0}", accesstoken));
                req.Accept = "application/json";
                using (var resp = await req.GetResponseAsync()) {
                    using (var sr = new StreamReader(resp.GetResponseStream())) {
                        return JsonConvert.DeserializeObject<dynamic>(await sr.ReadToEndAsync());
                    }
                }
            } catch (Exception ex) {
                throw;
            }
        }
    }
}
