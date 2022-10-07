namespace Microsoft.Extensions.Hosting
{
    public class OpenIdConnectServerOptions
    {
        public string SigningKey { set; get; } = "";

        public string EncryptionKey { set; get; } = "";
    }
}
