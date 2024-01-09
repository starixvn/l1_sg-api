using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SG.Core.Shared.Utils
{
    public class HMACUtils
    {
        public static string GenerateAccessKey(string secretKey, object request)
        {
            var requestBody = JsonConvert.SerializeObject(request);
            var hmac = new HMACSHA256(Encoding.ASCII.GetBytes(secretKey));
            var hashResult = hmac.ComputeHash(Encoding.ASCII.GetBytes(requestBody));

            return Convert.ToBase64String(hashResult);
        }
    }
}
