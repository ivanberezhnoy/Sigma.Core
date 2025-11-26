namespace Sigma.Core.DB.Entities
{
    public class RefreshTokenDbEntity
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;

        public string TokenHash { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; }
    }
}
