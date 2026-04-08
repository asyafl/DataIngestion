using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace DataIngestion.Application.Tests.Helpers
{
    internal static class FormFileFactory
    {
        public static IFormFile CreateCsv(string fileName, string content)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName);
        }
    }
}
