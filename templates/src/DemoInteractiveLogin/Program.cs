using DemoInteractiveLogin;
using IdentityModel.OidcClient;
using System.Text.Json;

// Emulate sign-in from Next.js
var port = 3000;
var redirectUriPath = "/api/auth/callback/oidc";
var browser = new SystemBrowser(port, redirectUriPath);

var options = new OidcClientOptions
{
    //Authority = "https://demo.duendesoftware.com",
    //ClientId = "interactive.public.short",
    //Scope = "openid profile email api offline_access",

    Authority = "http://localhost:5051",
    ClientId = "cms",
    Scope = "openid profile email roles offline_access api",

    RedirectUri = $"http://localhost:{port}{redirectUriPath}",
    Browser = browser,
    RefreshTokenInnerHttpHandler = new SocketsHttpHandler(),
    FilterClaims = false,
};

var oidcClient = new OidcClient(options);
var result = await oidcClient.LoginAsync(new LoginRequest());

if (result.IsError)
{
    Console.WriteLine("\n\nError:\n{0}", result.Error);
    return;
}

Console.WriteLine($"Access Token: {result.AccessToken}");
Console.WriteLine($"\n\nRefresh Token: {result.RefreshToken}");

Console.WriteLine("\n\nClaims:");
foreach (var claim in result.User.Claims)
{
    Console.WriteLine("{0}: {1}", claim.Type, claim.Value);
}

var apiClient = new HttpClient(result.RefreshTokenHandler)
{
    BaseAddress = new Uri("http://localhost:5052")
};

var response = await apiClient.GetAsync("/api/test");

if (response.IsSuccessStatusCode)
{
    var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    Console.WriteLine("\n\n");
    Console.WriteLine(json.RootElement);
}
else
{
    Console.WriteLine($"Error: {response.ReasonPhrase}");
}

Console.ReadLine();