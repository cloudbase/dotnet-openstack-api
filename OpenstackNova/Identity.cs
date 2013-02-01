
namespace Cloudbase.OpenstackNova
{
    public class Identity
    {
        public int Id { get; set; }

        public string Username { get; set; }

        public string TenantName { get; set; }

        public string Password { get; set; }

        public string TenantId { get; set; }

        public string AuthEndpoint { get; set; }

    }
}