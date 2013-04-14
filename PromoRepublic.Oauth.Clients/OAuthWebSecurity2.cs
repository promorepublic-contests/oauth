using System;
using System.Collections.Generic;
using Microsoft.Web.WebPages.OAuth;

namespace PromoRepublic.Oauth.Clients
{
    class OAuthWebSecurity2
    {
        public static void RegisterFacebookClient(string appId, string appSecret, IEnumerable<string> extraScopes = null)
        {
            if (!String.IsNullOrEmpty(appId) && !String.IsNullOrEmpty(appSecret))
                OAuthWebSecurity.RegisterClient(
                    new FbClient(appId, appSecret, extraScopes),
                    "Facebook",
                    new Dictionary<string, object>());            
        }
        
        public static void RegisterVkontakteClient(string appId, string appSecret)
        {
            if (!String.IsNullOrEmpty(appId) && !String.IsNullOrEmpty(appSecret))
                OAuthWebSecurity.RegisterClient(
                    new VkClient(appId, appSecret),
                    "Vkontakte",
                    new Dictionary<string, object>());            
        }   
     
        public static void RegisterOdnoklassnikiClient(string appId, string appSecret, string appPublic)
        {
            if (!String.IsNullOrEmpty(appId) && !String.IsNullOrEmpty(appSecret) && !String.IsNullOrEmpty(appPublic))
                OAuthWebSecurity.RegisterClient(
                    new OkClient(appId, appSecret, appPublic),
                    "Odnoklassniki",
                    new Dictionary<string, object>());            
        }
    }
}
