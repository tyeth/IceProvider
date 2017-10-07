using System.Net;

namespace IceProvider
{
    public static class Extensions
    {
        public static WebHeaderCollection AddOrUpdate(this WebHeaderCollection h, string header, string value)
        {
            h[header] = value;
            return h;
        }
    }
}