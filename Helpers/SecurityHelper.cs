using SpraywallAppMobile.Models;
using System.Security.Cryptography;


namespace SpraywallAppMobile.Helpers;

// Class containing methods related to security procedures
static class SecurityHelper
{
    // Encrypt data, using the public key retrieved from the api, stored in appsettings
    // Public, because public key cryptography
    public static byte[] Encrypt(byte[] data)
    {
        using (RSA rsaPublic = RSA.Create())
        {
            rsaPublic.FromXmlString(AppSettings.PublicKeyXML);
            return rsaPublic.Encrypt(data, RSAEncryptionPadding.Pkcs1);
        }
    }
}