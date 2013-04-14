using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using Demo.Models;
using PromoRepublic.Oauth.Clients;

namespace Demo
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            OAuthWebSecurity2.RegisterFacebookClient(appId: "367724710010097", appSecret: "c9946e8bd3d60f3d2c837bde0959f2a0");
            OAuthWebSecurity2.RegisterVkontakteClient(appId: "3574280", appSecret: "tERTT62G1g7HeXR1Pq4O");
            OAuthWebSecurity2.RegisterOdnoklassnikiClient(appId: "169393664", appSecret: "B155BD73E25BC31BAE90F506", appPublic: "CBAPLJBLABABABABA");

            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");

            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: "",
            //    appSecret: "");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
