
// https://github.com/openiddict/openiddict-samples/blob/dev/samples/Aridka/Aridka.Client/Program.cs

using IdentityModel.Client;
using System.Net.Http.Headers;
using System.Text;

using var client = new HttpClient();

try
{
    var token = await GetTokenAsync(client);
    Console.WriteLine("Access token: {0}", token);
    Console.WriteLine();

    var resource = await GetResourceAsync(client, token);
    Console.WriteLine("API response: {0}", resource);
    Console.ReadLine();
}
catch (HttpRequestException exception)
{
    var builder = new StringBuilder();
    builder.AppendLine("+++++++++++++++++++++");
    builder.AppendLine(exception.Message);
    builder.AppendLine(exception.InnerException?.Message);
    builder.AppendLine("Make sure you started the authorization server.");
    builder.AppendLine("+++++++++++++++++++++");
    Console.WriteLine(builder.ToString());
}

static async Task<string> GetTokenAsync(HttpClient client)
{
    // Retrieve the OpenIddict server configuration document containing the endpoint URLs.
    var configuration = await client.GetDiscoveryDocumentAsync("http://localhost:5051");
    if (configuration.IsError)
    {
        throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
    }

    var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
    {
        Address = configuration.TokenEndpoint,
        ClientId = "demo-m2m",
        ClientSecret = "HelloWorld1!",
        Scope = "demo-api"
    });

    if (response.IsError)
    {
        throw new Exception($"An error occurred while retrieving an access token: {response.Error}");
    }

    return response.AccessToken;
}

static async Task<string> GetResourceAsync(HttpClient client, string token)
{
    using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5010/api/values");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    using var response = await client.SendAsync(request);
    response.EnsureSuccessStatusCode();

    return await response.Content.ReadAsStringAsync();
}