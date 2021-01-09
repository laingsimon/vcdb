using System.Linq;
using System.Security.Cryptography;
using System.Text;
using vcdb.CommandLine;

namespace vcdb
{
    public class HashHelper : IHashHelper
    {
        private readonly Options options;

        public HashHelper(Options options)
        {
            this.options = options;
        }

        public string GetHash(string input, int? hashSize = null)
        {
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            var hashSizeToUse = hashSize ?? options.HashSize;
            return string.Join("", hash.Take(hashSizeToUse / 2).Select(b => b.ToString("X")));
        }
    }
}
