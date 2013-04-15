PromoRepublic.Oauth.Clients library
=====

Extensions for DotNetOpenAuth2 library to work with VKontakte and Odnoklassniki

Installation
------------

Either download the code and build it along with demo application, or install NuGet package

    Install-Package PromoRepublic.Oauth.Clients
    
Usage
-----

Add the following code lines to your AuthConfig.cs:

```csharp
     OAuthWebSecurity2.RegisterFacebookClient(appId: "...", appSecret: "...");
     OAuthWebSecurity2.RegisterVkontakteClient(appId: "...", appSecret: "...");
     OAuthWebSecurity2.RegisterOdnoklassnikiClient(appId: "...", appSecret: "...", appPublic: "...");
```

filling the blanks with your app Ids and keys. Good article of how to obtain this Ids here: [Article in russian](http://habrahabr.ru/post/145988/)
