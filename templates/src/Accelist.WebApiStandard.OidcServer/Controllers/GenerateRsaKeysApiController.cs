using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Accelist.WebApiStandard.OidcServer.Controllers
{
    public class GenerateRsaSigningKeyResponse
    {
        public string OidcSigningKey { set; get; } = "";

        public string OidcEncryptionKey { set; get; } = "";
    }

    [Route("api/generate-rsa-keys")]
    [ApiController]
    public class GenerateRsaKeysApiController : ControllerBase
    {
        [HttpGet]
        public ActionResult<GenerateRsaSigningKeyResponse> Get()
        {
            using var encryptionKey = RSA.Create(2048);
            using var signingKey = RSA.Create(2048);
            var encryptionKeyBits = encryptionKey.ExportRSAPrivateKey();
            var signingKeyBits = signingKey.ExportRSAPrivateKey();
            var result = new GenerateRsaSigningKeyResponse
            {
                OidcEncryptionKey = Convert.ToBase64String(encryptionKeyBits),
                OidcSigningKey = Convert.ToBase64String(signingKeyBits)
            };
            return result;
        }
    }
}
