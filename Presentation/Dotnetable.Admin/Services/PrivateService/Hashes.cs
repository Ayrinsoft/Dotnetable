using System.Text;

namespace Dotnetable.Service.PrivateService
{
    internal static class Hashes
    {
        internal static string HashLogin(this string stringToHash)
        {
            //Change this hash code
            byte[] bytesToHash = Encoding.UTF8.GetBytes($"H@$hBS7xcmm{stringToHash}J7s*M@va*`!@");
            using (var ShaObj = System.Security.Cryptography.SHA384.Create()) { bytesToHash = ShaObj.ComputeHash(bytesToHash); }
            var strResult = new StringBuilder();
            foreach (byte b in bytesToHash) strResult.Append(b.ToString("x2"));
            return strResult.ToString();
        }


    }
}
