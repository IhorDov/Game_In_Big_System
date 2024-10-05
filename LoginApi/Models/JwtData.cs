namespace LoginApi.Models
{
    public class JwtData
    {
        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }
}
