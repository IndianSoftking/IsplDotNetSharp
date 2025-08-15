namespace ISPL.NetCoreFramework.Helpers
{
    public static class CryptoHelper
    {
        private static readonly string KeyPassword = AppSettingsHelper.Get("JwtSetting:Key") ?? string.Empty;
        private const int KeySize = 256;
        private const int DerivationIterations = 100_000;

        public static string Encrypt(string input)
        {
            byte[] salt = RandomBytes(32);
            byte[] iv = RandomBytes(16);

            using var keyDerivation = new Rfc2898DeriveBytes(KeyPassword, salt, DerivationIterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize / 8);

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(input);
            byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            byte[] result = new byte[salt.Length + iv.Length + cipherBytes.Length];
            Buffer.BlockCopy(salt, 0, result, 0, salt.Length);
            Buffer.BlockCopy(iv, 0, result, salt.Length, iv.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, salt.Length + iv.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public static string Decrypt(string input)
        {
            byte[] encryptedData = Convert.FromBase64String(input);

            byte[] salt = new byte[32];
            byte[] iv = new byte[16];
            Buffer.BlockCopy(encryptedData, 0, salt, 0, salt.Length);
            Buffer.BlockCopy(encryptedData, salt.Length, iv, 0, iv.Length);

            byte[] cipherBytes = new byte[encryptedData.Length - salt.Length - iv.Length];
            Buffer.BlockCopy(encryptedData, salt.Length + iv.Length, cipherBytes, 0, cipherBytes.Length);

            using var keyDerivation = new Rfc2898DeriveBytes(KeyPassword, salt, DerivationIterations, HashAlgorithmName.SHA256);
            byte[] key = keyDerivation.GetBytes(KeySize / 8);

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        private static byte[] RandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }
    }

}
