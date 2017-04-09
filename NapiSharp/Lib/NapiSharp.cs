using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NapiSharp.Lib
{
    public class NapiSharp
    { 
        private const string nick = "";
        private const string pass = "";
        private const string lang = "PL";
        private const string os = "Windows";
        private const int NAPI_10MB = 10485760;
        private const string NAPI_7XIP_PASS = "iBlm8NTigvru0Jr0";

        private string napiDownloadUrl =
            "http://www.napiprojekt.pl/unit_napisy/dl.php?l={0}&f={1}&t={2}&v=other&kolejka=false&nick={3}&pass={4}&napios={5}";


        
        public string DownloadSubtitle(string movieFilePath)
        {
            var tmpPath = Path.GetTempPath() + Guid.NewGuid() + "\\";
            Directory.CreateDirectory(tmpPath);

            var downloaded7z = "napi.7z";
            
            var movieDir = Path.GetDirectoryName(movieFilePath);
            var movieName = Path.GetFileNameWithoutExtension(movieFilePath);
            var destinationSubtitle = movieDir + "\\" + movieName + ".txt";
            
            var checksum = CalcChecksum(movieFilePath, true);

            if (DownloadByChecksum(lang, checksum, tmpPath + downloaded7z))
            {
                Exract7Zip(tmpPath + downloaded7z, tmpPath);

                File.Copy(tmpPath + checksum + ".txt", destinationSubtitle, true);

                return destinationSubtitle;
            }

            return "";
        }

        private bool DownloadByChecksum(string lang, string checksum, string pathToDownloadedFile)
        {
            var f = FDigest(checksum);

            var url = string.Format(napiDownloadUrl, lang, checksum, f, nick, pass, os);
            using (var client = new WebClient())
            {
                //client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
                client.Headers.Add(HttpRequestHeader.AcceptLanguage, "pl-PL,en,*");
                client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0");
                client.DownloadFile(url, pathToDownloadedFile);
            }

            var buffer = new byte[3];
            using (var fs = new FileStream(pathToDownloadedFile, FileMode.Open, FileAccess.Read))
            {
                fs.Read(buffer, 0, buffer.Length);
                fs.Close();
                var str = Encoding.UTF8.GetString(buffer);
                if (str == "NPc")
                    return false;
            }
            return true;
        }

        private string CalcChecksum(string filename, bool limit10m)
        {
            using (var md5 = MD5.Create())
            {
                if (limit10m)
                {
                    using (var fs = new FileStream(filename, FileMode.Open))
                    {
                        var buffer = new byte[NAPI_10MB];
                        fs.Read(buffer, 0, buffer.Length);
                        return ToHex(md5.ComputeHash(buffer));
                    }
                }
                else
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        return ToHex(md5.ComputeHash(stream));
                    }
                }
            }
        }

        private string ToHex(byte[] bytes)
        {
            var result = new StringBuilder(bytes.Length * 2);

            foreach (var t in bytes)
                result.Append(t.ToString("x2"));

            return result.ToString();
        }

        private string FDigest(string input)
        {
            if (input.Length != 32) return "";

            int[] idx = {0xe, 0x3, 0x6, 0x8, 0x2};
            int[] mul = {2, 2, 5, 4, 3};
            int[] add = {0x0, 0xd, 0x10, 0xb, 0x5};

            string b = "";

            for (var j = 0; j <= 4; j++)
            {
                var a = add[j];
                var m = mul[j];
                var i = idx[j];
                var t = a + int.Parse(input[i]+"", NumberStyles.HexNumber); 
                var v = int.Parse(t == 31 ? input.Substring(t, 1) : input.Substring(t, 2), NumberStyles.HexNumber);
                var x = v * m % 0x10;
                b += x.ToString("x");
            }

            return b;
        }



        private void Exract7Zip(string file7z, string destinationPath)
        {
            var tmpP = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string zPath = tmpP + @"\7za.exe";
            
            try
            {
                var pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = zPath;
                pro.Arguments = "e -y -p" + NAPI_7XIP_PASS + " -o\"" + destinationPath + "\" \"" + file7z + "\"";
                var x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
