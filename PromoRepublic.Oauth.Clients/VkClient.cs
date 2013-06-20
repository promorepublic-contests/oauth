using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Web;
using DotNetOpenAuth.AspNet.Clients;

namespace PromoRepublic.Oauth.Clients
{
    public class VkClient : OAuth2Client
    {
        private readonly string _appId;
        private readonly string _appSecret;
 
        public VkClient(string appId, string appSecret) : base("vkontakte") {
            if (String.IsNullOrEmpty(appId)) throw new ArgumentNullException("appId");
            if (String.IsNullOrEmpty(appSecret)) throw new ArgumentNullException("appSecret");
            _appId = appId;
            _appSecret = appSecret;
        }

        public VkClient(string appId, string appSecret, IEnumerable<string> extraScopes)
            : this(appId, appSecret)
        {
            if (extraScopes != null)
            {
                Scopes.AddRange(extraScopes);
            }
        }

        public string AppId
        {
            get { return _appId; }
        }

        public string AppSecret
        {
            get { return _appSecret; }
        }

        private readonly List<string> _scopes = new List<string>(new []{"email"});

        /// <summary>
        /// notify,friends,photos,audio,video,docs,notes,pages,wall,groups,messages,ads,offline
        /// </summary>
        public List<string> Scopes { get { return _scopes; } }
        
        protected override Uri GetServiceLoginUrl(Uri returnUrl) {
            if (returnUrl == null) throw new ArgumentNullException("returnUrl");
            var builder = new UriBuilder("https://oauth.vk.com/authorize");
            var args = new Dictionary<string, string>
                {
                    {"client_id", AppId},
                    {"redirect_uri", NormalizeHexEncoding(returnUrl.AbsoluteUri)},
                    {"display", "page"},
                    {"response_type", "code"},
                    {"scope", String.Join(",", Scopes)},
                };
            builder.AppendQueryArgs(args);
            return builder.Uri;
        }
 
        protected override IDictionary<string, string> GetUserData(string accessToken) {
            if (accessToken == null) throw new ArgumentNullException("accessToken");
            var data = HttpContext.Current.Session["VKontakteOAuthProvider.Token"] as VKontakteTokenResponse;
            if (data == null || data.AccessToken != accessToken)
                return null;
            var res = new Dictionary<string, string>
                {
                    {"id", data.UserId.ToString(CultureInfo.InvariantCulture)}
                };
            var builder = new UriBuilder("https://api.vk.com/method/users.get");
            var args = new Dictionary<string, string>
                {
                    {"uids", data.UserId.ToString(CultureInfo.InvariantCulture)},
                    {"fields", "uid,first_name,last_name,screen_name,nickname,sex,bdate,photo_big,photo,contacts"},
                    {"access_token", accessToken}
                };
            builder.AppendQueryArgs(args);
            using (var client = new WebClient()) {
                using (var stream = client.OpenRead(builder.Uri)) {
                    var serializer = new DataContractJsonSerializer(typeof(VKontakteDataResponse));
                    if (stream != null)
                    {
                        var info = (VKontakteDataResponse)serializer.ReadObject(stream);
                        if (info.Items != null && info.Items.Any()) {
                            var item = info.Items[0];
                            res.AddItemIfNotEmpty("username", item.Username);
                            res.AddItemIfNotEmpty("name", (((item.FirstName ?? "") + " " + (item.LastName ?? "")).Trim()));
                            res.AddItemIfNotEmpty("birthday", item.Birthdate);
                            res.AddItemIfNotEmpty("gender", item.Sex);
                            res.AddItemIfNotEmpty("photo", item.PhotoBig ?? item.Photo);
                            res.AddItemIfNotEmpty("phone", item.PhoneMobile ?? item.PhoneHome ?? item.Phone);
                        }
                    }
                }
            }
            return res;
        }
 
        protected override string QueryAccessToken(Uri returnUrl, string authorizationCode) {
            var builder = new UriBuilder("https://oauth.vk.com/access_token");
            var args = new Dictionary<string, string>
                {
                    {"client_id", AppId},
                    {"redirect_uri", NormalizeHexEncoding(returnUrl.AbsoluteUri)},
                    {"client_secret", AppSecret},
                    {"code", authorizationCode}
                };
            builder.AppendQueryArgs(args);
            using (var client = new WebClient()) {
                using (var stream = client.OpenRead(builder.Uri)) {
                    var serializer = new DataContractJsonSerializer(typeof(VKontakteTokenResponse));
                    if (stream != null)
                    {
                        var data = (VKontakteTokenResponse)serializer.ReadObject(stream);
                        HttpContext.Current.Session["VKontakteOAuthProvider.Token"] = data;
                        return data.AccessToken;
                    }
                }
            }

            return null;
        }
 
        private static string NormalizeHexEncoding(string url) {
            char[] chArray = url.ToCharArray();
            for (int i = 0; i < (chArray.Length - 2); i++) {
                if (chArray[i] == '%') {
                    chArray[i + 1] = char.ToUpperInvariant(chArray[i + 1]);
                    chArray[i + 2] = char.ToUpperInvariant(chArray[i + 2]);
                    i += 2;
                }
            }
            return new string(chArray);
        }
    }
 
    [DataContract, EditorBrowsable(EditorBrowsableState.Never)]
    internal class VKontakteTokenResponse {
        [DataMember(Name = "access_token")]
        public string AccessToken { get; set; }
        [DataMember(Name = "user_id")]
        public long UserId { get; set; }
        [DataMember(Name = "expires_in")]
        public long ExpiresIn { get; set; }
    }
 
    [DataContract, EditorBrowsable(EditorBrowsableState.Never)]
    internal class VKontakteDataResponse {
        [DataMember(Name = "response")]
        public VKontakteDataItem[] Items { get; set; }
    }
 
    [DataContract, EditorBrowsable(EditorBrowsableState.Never)]
    internal class VKontakteDataItem {
        [DataMember(Name = "uid")]
        public long UserId { get; set; }
        [DataMember(Name = "sex")]
        public string Sex { get; set; }
        [DataMember(Name = "screen_name")]
        public string Username { get; set; }
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }
        [DataMember(Name = "last_name")]
        public string LastName { get; set; }
        [DataMember(Name = "bdate")]
        public string Birthdate { get; set; }
        [DataMember(Name = "photo")]
        public string Photo { get; set; }
        [DataMember(Name = "photo_big")]
        public string PhotoBig { get; set; }
        [DataMember(Name = "phone")]
        public string Phone { get; set; }
        [DataMember(Name = "mobile_phone")]
        public string PhoneMobile { get; set; }
        [DataMember(Name = "home_phone")]
        public string PhoneHome { get; set; }
    }
}
