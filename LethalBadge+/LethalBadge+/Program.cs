using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using Leaf.xNet;

namespace LethalBadge_
{
    internal class Program
    {
        public static string Password = "lcslime14a5";
        public static string LocalLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow");
        public static string GameSavePath = LocalLowPath + "\\ZeekerssRBLX\\Lethal Company\\";
        private static Dictionary<string, string> levels = new Dictionary<string, string>();
        private static Dictionary<string, string> xp_values = new Dictionary<string, string>();

        private static byte[] Encrypt(string password, string data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;
                aesAlg.KeySize = 128;

                // Generate a random IV
                aesAlg.GenerateIV();

                using (Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, aesAlg.IV, 100, HashAlgorithmName.SHA1))
                {
                    aesAlg.Key = keyDerivation.GetBytes(16); // 128-bit key

                    using (MemoryStream encryptedStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(encryptedStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                            cryptoStream.Write(dataBytes, 0, dataBytes.Length);
                        }

                        byte[] iv = aesAlg.IV; // Get IV
                        byte[] encryptedData = encryptedStream.ToArray(); // Get encrypted data

                        // Combine IV and encrypted data
                        byte[] ivAndEncryptedData = new byte[iv.Length + encryptedData.Length];
                        Array.Copy(iv, 0, ivAndEncryptedData, 0, iv.Length);
                        Array.Copy(encryptedData, 0, ivAndEncryptedData, iv.Length, encryptedData.Length);

                        return ivAndEncryptedData;
                    }
                }
            }
        }
        private static string Decrypt(string password, byte[] data)
        {
            byte[] IV = new byte[16];
            Array.Copy(data, IV, 16);
            byte[] dataToDecrypt = new byte[data.Length - 16];
            Array.Copy(data, 16, dataToDecrypt, 0, dataToDecrypt.Length);

            using (Rfc2898DeriveBytes k2 = new Rfc2898DeriveBytes(password, IV, 100, HashAlgorithmName.SHA1))
            using (Aes decAlg = Aes.Create())
            {
                decAlg.Mode = CipherMode.CBC;
                decAlg.Padding = PaddingMode.PKCS7;
                decAlg.Key = k2.GetBytes(16);
                decAlg.IV = IV;

                using (MemoryStream decryptionStreamBacking = new MemoryStream())
                using (CryptoStream decrypt = new CryptoStream(decryptionStreamBacking, decAlg.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    decrypt.Write(dataToDecrypt, 0, dataToDecrypt.Length);
                    decrypt.FlushFinalBlock();

                    return new UTF8Encoding(true).GetString(decryptionStreamBacking.ToArray());
                }
            }
        }
       
        static void Main(string[] args)
        {
            levels.Add("0", "Intern");
            levels.Add("1", "Part-Time");
            levels.Add("2", "Employee");
            levels.Add("3", "Leader");
            levels.Add("4", "Boss");
            levels.Add("5", "Empty");


            xp_values.Add("0", "49");
            xp_values.Add("1", "99");
            xp_values.Add("2", "199");
            xp_values.Add("3", "499");
            xp_values.Add("4", "1000");

        JUMP1:
            
            string decrypted_save_file = Decrypt(Password, File.ReadAllBytes(GameSavePath + "\\LCGeneralSaveData"));
            string curLevel = decrypted_save_file.Substring("PlayerLevel\":{\"__type\":\"int\",\"value\":", "}");
            string curXP = decrypted_save_file.Substring("PlayerXPNum\":{\"__type\":\"int\",\"value\":", "}");

            Console.Write("Current Level: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(levels[curLevel] + "\n\n");
            Console.ResetColor();

            Console.WriteLine("[0] Intern");
            Console.WriteLine("[1] Part-Time");
            Console.WriteLine("[2] Employee");
            Console.WriteLine("[3] Leader");
            Console.WriteLine("[4] Boss");

            Console.Write("Choose desired Level: ");
            
            string desiredLevel = Console.ReadLine();

            decrypted_save_file = decrypted_save_file.Replace("\"PlayerLevel\":{\"__type\":\"int\",\"value\":" + curLevel + "}", "\"PlayerLevel\":{\"__type\":\"int\",\"value\":" + desiredLevel + "}");
            decrypted_save_file = decrypted_save_file.Replace("PlayerXPNum\":{\"__type\":\"int\",\"value\":" + curXP + "}", "PlayerXPNum\":{\"__type\":\"int\",\"value\":" + xp_values[desiredLevel] + "}");

            File.WriteAllBytes(GameSavePath + "\\LCGeneralSaveData", Encrypt(Password, decrypted_save_file));
            Console.WriteLine("Level changed!");
            Console.ReadKey();
            goto JUMP1;
        }
    }
}
