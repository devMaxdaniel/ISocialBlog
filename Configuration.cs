namespace ISocialBlog;

public static class Configuration
{
    public static string JwtKey = "ZmVKYWY3GTy7KJdfqPDM0iTrWWQ1";
    public static string ApiKeyName = "api_key";
    public static string Apikey = "curso_api_IlteVUM;a=zz0";
    public static SmtpConfiguration Smtp = new();


    public class SmtpConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; } = 25;
        public string UserName { get; set; }
        public string Password { get; set; }

    }

}

