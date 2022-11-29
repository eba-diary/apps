using Sentry.Common.Logging;
using Sentry.data.Core;
using System;
using System.IO;
using System.Security.Cryptography;
using static Sentry.data.Core.GlobalConstants;

namespace Sentry.data.Infrastructure
{
    public class EncryptionService : IEncryptionService
    {
        public EncryptionService() { }
        /// <summary>
        /// Generate an encrypted value and return a new initial value with each encryption.
        /// Returns a tuple (Item1 = encrypted\decrypted string, Item2 = inital value used to seed the encryption).
        /// IV value is required to return the decrypted value using the DecryptString method.
        /// </summary>
        /// <param name="inputString"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public Tuple<string,string> EncryptString(string inputString, string key, string iv = null)
        {
            try
            {
                using (AesManaged myAes = new AesManaged())
                {
                    //Set key
                    myAes.Key = Convert.FromBase64String(key);

                    if (iv == null)
                    {
                        myAes.GenerateIV();
                    }
                    else
                    {
                        myAes.IV = Convert.FromBase64String(iv);
                    }

                    byte[] encrypted = EncryptStringToBytes_Aes(inputString, myAes.Key, myAes.IV);

                    return Tuple.Create(Convert.ToBase64String(encrypted), Convert.ToBase64String(myAes.IV));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to Encrypt value", ex);
                throw;
            }            
        }

        public string DecryptString(string inputString, string key, string IV)
        {
            try
            {
                using (AesManaged myAes = new AesManaged())
                {
                    //Set key
                    myAes.Key = Convert.FromBase64String(key);

                    //Generate IV if one is not passed
                    if (IV == null || IV.Length == 0)
                    {
                        throw new ArgumentNullException("IV");
                    }
                    else
                    {
                        myAes.IV = Convert.FromBase64String(IV);
                    }

                    return DecryptStringFromBytes_Aes(Convert.FromBase64String(inputString), myAes.Key, myAes.IV);

                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to Encrypt value", ex);
                throw;
            }
        }

        public string GenerateNewIV()
        {
            using (AesManaged myAes = new AesManaged())
            {
                myAes.GenerateIV();

                return Convert.ToBase64String(myAes.IV);
            }
        }

        /// <summary>
        /// Decrypts existing value with old IV and encrypts value with new IV, using same key.  Returns new encrypted value.
        /// </summary>
        /// <param name="origValue"></param>
        /// <param name="key"></param>
        /// <param name="oldIv"></param>
        /// <param name="newIv"></param>
        /// <returns></returns>
        public string DecryptEncryptUsingNewIV(string origValue, string key, string oldIv, string newIv)
        {
            return EncryptString(DecryptString(origValue, key, oldIv), key, newIv).Item1;
        }

        public bool IsEncrypted(string input)
        {
            return !string.IsNullOrEmpty(input) && input.StartsWith(Encryption.ENCRYPTIONINDICATOR) && input.EndsWith(Encryption.ENCRYPTIONINDICATOR);
        }

        public string PrepEncryptedForDisplay(string encryptedString)
        {
            if (!string.IsNullOrEmpty(encryptedString))
            {
                return Encryption.ENCRYPTIONINDICATOR + encryptedString + Encryption.ENCRYPTIONINDICATOR;
            }

            return encryptedString;
        }

        private byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                //aesAlg.KeySize = 256;
                //aesAlg.Mode = CipherMode.
                //aesAlg.Padding = PaddingMode.;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                //aesAlg.KeySize = 256;
                //aesAlg.Padding = PaddingMode.None;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}
