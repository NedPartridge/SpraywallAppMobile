using System.Security.Cryptography;

namespace SpraywallAppMobile.Models;
class BaseUser
{
    public string Email { get; set; }
    byte[] Password { get; set; }

    public BaseUser(string email, byte[] password)
    {
        Email = email;
        Password = password;
    }

    public byte[] HashedPassword
    {
        get
        {
            return Encrypt(Password);
        }
    }

    // Encrypt the password
    // Shamelessly ripped from examples on microsoft's website:
    // https://learn.microsoft.com/en-us/dotnet/standard/security/encrypting-data
    byte[] Encrypt(byte[] password)
    {
        // Create values to store encrypted symmetric keys.
        byte[] encryptedSymmetricKey;
        byte[] encryptedSymmetricIV;

        // Initialise RSA
        RSA rsa = RSA.Create();
        RSAParameters rsaKeyInfo = new RSAParameters();

        // Public keys - to be coordinated with the API, when that's done 
        // Purely here for demonstrative purposes, right now.
        rsaKeyInfo.Modulus = new byte[] {
            214,46,220,83,160,73,40,39,201,155,19,202,3,11,191,178,56,
            74,90,36,248,103,18,144,170,163,145,87,54,61,34,220,222,
            207,137,149,173,14,92,120,206,222,158,28,40,24,30,16,175,
            108,128,35,230,118,40,121,113,125,216,130,11,24,90,48,194,
            240,105,44,76,34,57,249,228,125,80,38,9,136,29,117,207,139,
            168,181,85,137,126,10,126,242,120,247,121,8,100,12,201,171,
            38,226,193,180,190,117,177,87,143,242,213,11,44,180,113,93,
            106,99,179,68,175,211,164,116,64,148,226,254,172,147
        };
        rsaKeyInfo.Exponent = new byte[] { 1, 0, 1 };
        rsa.ImportParameters(rsaKeyInfo);
        Aes aes = Aes.Create();

        // Encrypt the symmetric key and IV.
        encryptedSymmetricKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
        encryptedSymmetricIV = rsa.Encrypt(aes.IV, RSAEncryptionPadding.Pkcs1);

        // Encrypt the password using AES.
        ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] encryptedPassword = encryptor.TransformFinalBlock(password, 0, password.Length);

        return encryptedPassword;
    }
}