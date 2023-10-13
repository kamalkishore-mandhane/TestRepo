using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace ImgUtil
{
    class AsposeLicense
    {
        //private static string publickey = @"<RSAKeyValue><Modulus>rnMl9BAJZzDh3u/dV1wpyXuV364uiwnNejUyQcOVyXljbT+/qjMOWGluVwdt+aDJMXLsAMg32Uc+Z0nx/V+wnNO+r/yFzTWpBpQAye6/wb89g+yW5K3AGfzg90nlgPEi5kfj3k7SdA7zW6fmxWU26zVux/NZVAp+Niger7M9hfk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private static string privatekey = @"<RSAKeyValue><Modulus>rnMl9BAJZzDh3u/dV1wpyXuV364uiwnNejUyQcOVyXljbT+/qjMOWGluVwdt+aDJMXLsAMg32Uc+Z0nx/V+wnNO+r/yFzTWpBpQAye6/wb89g+yW5K3AGfzg90nlgPEi5kfj3k7SdA7zW6fmxWU26zVux/NZVAp+Niger7M9hfk=</Modulus><Exponent>AQAB</Exponent><P>yn3bNeKb3eNqDoyhDcA1fafz4WLCHr7/gjPmqyEseXFtjKs1PSNjUtZHD1jr/ru0G+y9FMiVFNNscvvKQDtWcw==</P><Q>3IxR2YR5E2808dtKEWEOhbF4bP63G9vElCV9DwEot9Jh6Eqo2sqcNtACok/lB4ac/kC7tpJGOBUe0p22Josq4w==</Q><DP>o6KbYGtVLDXYAhPxHqyiTX5JXm0xlCkjUDPjB44SY72fGttMdbDAVjPlTui8JanIPfzNPBtwJllIvY7ufYO2Mw==</DP><DQ>2s7sNZ3UcY+XO4yQg4WDXuifzaM4D4+ODFzVIhnIR/eV41yPAeKZ8VeWBWq2kyzefPHESnH88I8jsVl+6eaQeQ==</DQ><InverseQ>w0ECgOMX275e7HjCWSWMvwPDpU3YkHYVSCvR7XvcBvEu3W8/1DoCUeoA9yt9Tw51LGSwmOX9go3xRvzhGPANAA==</InverseQ><D>iivh78GT8QuimzVZFwyEfHVKa/RGIRIOkbD4sWX8iat/uNQ5NtFhl11Ka9wSmxliwavIiYYL1ii7oIvNA2Z7NyzdfKXbIw/dcARTnIbNfcEIFDzXCtgUxkjlMVc4uroqNKW0pK7VnATI2mUzBQ8Apd7Yh52hzHg8xzDLVH839ZE=</D></RSAKeyValue>";
        private static Stream licStream;
        private static Mutex licMutex = new Mutex(false, "licenseMutex");

        public static string RSADecrypt(string decryptstring)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] dataEnc = Convert.FromBase64String(decryptstring);
            rsa.FromXmlString(privatekey);
            int keySize = rsa.KeySize >> 3;
            string result = string.Empty;
            if (dataEnc.Length > keySize)
                result = RSADecryptLong(rsa, dataEnc, keySize);
            else
                result = RSADecryptNormal(rsa, dataEnc);
            return result;
        }

        private static string RSADecryptNormal(RSACryptoServiceProvider rsa, byte[] dataEnc)
        {
            byte[] DypherTextBArray = rsa.Decrypt(dataEnc, false);
            return (new UnicodeEncoding()).GetString(DypherTextBArray);
        }

        private static string RSADecryptLong(RSACryptoServiceProvider rsa, byte[] dataEnc, int keySize)
        {
            byte[] buffer = new byte[keySize];
            MemoryStream msInput = new MemoryStream(dataEnc);
            MemoryStream msOutput = new MemoryStream();
            int readLen = msInput.Read(buffer, 0, keySize);
            while (readLen > 0)
            {
                byte[] dataToDec = new byte[readLen];
                Array.Copy(buffer, 0, dataToDec, 0, readLen);
                byte[] decData = rsa.Decrypt(dataToDec, false);
                msOutput.Write(decData, 0, decData.Length);
                readLen = msInput.Read(buffer, 0, keySize);
            }
            byte[] result = msOutput.ToArray();
            return (new UnicodeEncoding()).GetString(result);
        }

        private static void Decrypt()
        {
            try
            {
                StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(StringManager.AsposeLic)));
                byte[] decryptBytes = System.Text.Encoding.UTF8.GetBytes(RSADecrypt(reader.ReadToEnd()));
                licStream = new MemoryStream(decryptBytes);
                licStream.Position = 0;
                reader.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static bool Init()
        {
            try
            {
                licMutex.WaitOne();
                Decrypt();
                var imageLicense = new Aspose.Imaging.License();
                imageLicense.SetLicense(licStream);
                licStream.Position = 0;
                var psdLicense = new Aspose.PSD.License();
                psdLicense.SetLicense(licStream);
                licStream.Position = 0;
                licMutex.ReleaseMutex();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
