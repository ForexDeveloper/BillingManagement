using System;

namespace Shared.Utilities.Extensions
{
    public static class FileExtensions
    {
        public static string ConvertToBase64DataUrl(this byte[] byteArray, string contentType)
        {
            string base64String = Convert.ToBase64String(byteArray);
            return $"data:{contentType};base64,{base64String}";
        }
    }
}
