namespace Nop.Admin.Infrastructure
{
    public static class ImageLazyLoadExtension
    {
        public static string ToLazyLoadImageHtml(this string source)
        {
            return source.Replace("src", "data-original");
        }

        public static string ToOriginalImageHtml(this string source)
        {
            return source.Replace("data-original", "src");
        }
    }
}