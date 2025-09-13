namespace Shared.IdentityServerProvider.Configurations
{
    public class IdentityServerConfiguration
    {
        public Uri IdpServer { get; set; }

        public bool ValidateAudience { get; set; }
        public bool ValidateIssuer { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
    }
}
