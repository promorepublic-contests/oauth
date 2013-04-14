using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;

namespace PromoRepublic.Oauth.Clients
{
    public class OkClient : OAuth2Client
    {
        private readonly string _appId;
        private readonly string _appSecret;
        private readonly string _appPublic;

        public OkClient(string appId, string appSecret, string appPublic)
            : base("odnoklassniki")
        {
            if (String.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");
            if (String.IsNullOrEmpty(appSecret)) throw new ArgumentNullException("appSecret");
            if (String.IsNullOrEmpty(appPublic)) throw new ArgumentNullException("appPublic");
            _appId = appId;
            _appSecret = appSecret;
            _appPublic = appPublic;
        }

        public string AppId
        {
            get { return _appId; }
        }

        public string AppSecret
        {
            get { return _appSecret; }
        }

        public string AppPublic
        {
            get { return _appPublic; }
        }

        protected override Uri GetServiceLoginUrl(Uri returnUrl)
        {
            if (returnUrl == null) throw new ArgumentNullException("returnUrl");
            var builder = new UriBuilder("http://www.odnoklassniki.ru/oauth/authorize");
            var args = new Dictionary<string, string>
                {
                    {"client_id", AppId},
                    {"redirect_uri", returnUrl.AbsoluteUri},
                    {"scope", ""},
                    {"response_type", "code"}
                };
            builder.AppendQueryArgs(args);
            return builder.Uri;
        }

        protected override IDictionary<string, string> GetUserData(string accessToken)
        {
            if (accessToken == null) throw new ArgumentNullException("accessToken");
            var data = HttpContext.Current.Session["OdnoklassnikiOAuthProvider.Token"] as OdnoklassnikiTokenResponse;
            if (data == null || data.AccessToken != accessToken)
                return null;
            var res = new Dictionary<string, string>();
            var builder = new UriBuilder("http://api.odnoklassniki.ru/fb.do");
            //$curl = curl_init('http://api.odnoklassniki.ru/fb.do?access_token=' . $auth['access_token'] . 
            //'&application_key=' . $AUTH['application_key'] . '&method=users.getCurrentUser&sig=' . 
            //md5('application_key=' . $AUTH['application_key'] . 'method=users.getCurrentUser' . md5($auth['access_token'] . $AUTH['client_secret'])));

            var sign = String.Format("{0}{1}", accessToken, AppSecret).CalculateMd5Hash().ToLower();
            sign = String.Format("application_key={0}method=users.getCurrentUser{1}", AppPublic, sign).CalculateMd5Hash().ToLower();
            var args = new Dictionary<string, string>
                {
                    {"method", "users.getCurrentUser"},
                    {"access_token", accessToken},
                    {"application_key", AppPublic},
                    {"sig", sign},
                };
            builder.AppendQueryArgs(args);
            using (var client = new WebClient())
            {
                using (var stream = client.OpenRead(builder.Uri))
                {
                    var serializer = new DataContractJsonSerializer(typeof (OdnoklassnikiDataItem));
                    if (stream != null)
                    {
                        var info = (OdnoklassnikiDataItem) serializer.ReadObject(stream);
                        if (info != null)
                        {
                            var item = info;
                            res.AddItemIfNotEmpty("id", item.UserId);
                            res.AddItemIfNotEmpty("username", item.UserId);
                            res.AddItemIfNotEmpty("name",
                                                  item.Name ??
                                                  (((item.FirstName ?? "") + " " + (item.LastName ?? "")).Trim()));
                            res.AddItemIfNotEmpty("birthday", item.Birthdate);
                            res.AddItemIfNotEmpty("gender", item.Sex);
                            res.AddItemIfNotEmpty("link", String.Format("http://odnoklassniki.ru/profile/{0}", item.UserId));
                            res.AddItemIfNotEmpty("photo", item.Photo ?? item.Photo2);
                        }
                    }
                }
            }
            return res;
        }

        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode)
        {
            var builder = new UriBuilder("http://api.odnoklassniki.ru/oauth/token.do");

            //code - код авторизации, полученный в ответном адресе URL пользователя
            //redirect_uri - тот же URI для переадресации, который был указан при первом вызове
            //grant_type - _на данный момент поддерживается только код авторизации authorization_code
            //client_id - идентификатор приложения
            //client_secret - секретный ключ приложения

            var args = new NameValueCollection
                {
                    {"code", authorizationCode},
                    {"redirect_uri", returnUrl.AbsoluteUri},
                    {"grant_type", "authorization_code"},
                    {"client_id", AppId},
                    {"client_secret", AppSecret},
                };
            using (var client = new WebClient())
            {
                using (var stream = new MemoryStream(client.UploadValues(builder.Uri, "POST", args)))
                {
                    var serializer = new DataContractJsonSerializer(typeof (OdnoklassnikiTokenResponse));
                    var data = (OdnoklassnikiTokenResponse) serializer.ReadObject(stream);
                    HttpContext.Current.Session["OdnoklassnikiOAuthProvider.Token"] = data;
                    return data.AccessToken;
                }
            }
        }
    }

    //{"token_type":"session","refresh_token":"d5470467e45b73de2d3a6131ec97b31d69d1c3a4_214018074825_1367","access_token":"6ripa.5-03av6hji01y3x4g5v5k6f3k2xbsb"}
    [DataContract, EditorBrowsable(EditorBrowsableState.Never)]
    internal class OdnoklassnikiTokenResponse {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "token_type")]
        public string TokenType { get; set; }
        [DataMember(Name = "refresh_token")]
        public string RefreshToken { get; set; }
    }
 
    [DataContract, EditorBrowsable(EditorBrowsableState.Never)]
    internal class OdnoklassnikiDataItem {
        [DataMember(Name = "uid")]
        public string UserId { get; set; }
        [DataMember(Name = "gender")]
        public string Sex { get; set; }
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }
        [DataMember(Name = "last_name")]
        public string LastName { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "birthday")]
        public string Birthdate { get; set; }
        [DataMember(Name = "age")]
        public int Age { get; set; }
        [DataMember(Name = "pic_1")]
        public string Photo { get; set; }
        [DataMember(Name = "pic_2")]
        public string Photo2 { get; set; }
        [DataMember(Name = "has_email")]
        public bool HasEmail { get; set; }
    }
}
