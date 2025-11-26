using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Sigma.Core.DB.Entities;

namespace Sigma.Core.DB

{
    public interface IRefreshTokenRepository
    {
        void Save(RefreshTokenDbEntity token);
        RefreshTokenDbEntity? FindByHash(string tokenHash);
        void Revoke(RefreshTokenDbEntity token);
        void RevokeAllForUser(string userName);
    }

    public class MySqlRefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<MySqlRefreshTokenRepository> _logger;

        public MySqlRefreshTokenRepository(
            IOptions<DatabaseOptions> dbOptions,
            ILogger<MySqlRefreshTokenRepository> logger)
        {
            _connectionString = dbOptions.Value.ConnectionString;
            _logger = logger;
        }

        private MySqlConnection CreateConnection()
            => new MySqlConnection(_connectionString);

        public void Save(RefreshTokenDbEntity token)
        {
            const string sql = @"
                INSERT INTO user_refresh_tokens
                    (user_name, token_hash, created_at, expires_at, revoked_at, device_info)
                VALUES
                    (@user_name, @token_hash, @created_at, @expires_at, @revoked_at, @device_info);";

            using var conn = CreateConnection();
            conn.Open();

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user_name", token.UserName);
            cmd.Parameters.AddWithValue("@token_hash", token.TokenHash);
            cmd.Parameters.AddWithValue("@created_at", token.CreatedAt);
            cmd.Parameters.AddWithValue("@expires_at", token.ExpiresAt);
            cmd.Parameters.AddWithValue("@revoked_at", (object?)token.RevokedAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@device_info", (object?)token.DeviceInfo ?? DBNull.Value);

            cmd.ExecuteNonQuery();
        }

        public RefreshTokenDbEntity? FindByHash(string tokenHash)
        {
            const string sql = @"
                    SELECT id, user_name, token_hash, created_at, expires_at, revoked_at, device_info
                    FROM user_refresh_tokens
                    WHERE token_hash = @token_hash
                    LIMIT 1;";

            using var conn = CreateConnection();
            conn.Open();

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@token_hash", tokenHash);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            // Берём индексы колонок один раз
            int idOrdinal = reader.GetOrdinal("id");
            int userNameOrdinal = reader.GetOrdinal("user_name");
            int tokenHashOrdinal = reader.GetOrdinal("token_hash");
            int createdAtOrdinal = reader.GetOrdinal("created_at");
            int expiresAtOrdinal = reader.GetOrdinal("expires_at");
            int revokedAtOrdinal = reader.GetOrdinal("revoked_at");
            int deviceInfoOrdinal = reader.GetOrdinal("device_info");

            var entity = new RefreshTokenDbEntity
            {
                Id = reader.GetInt32(idOrdinal),
                UserName = reader.GetString(userNameOrdinal),
                TokenHash = reader.GetString(tokenHashOrdinal),
                CreatedAt = reader.GetDateTime(createdAtOrdinal),
                ExpiresAt = reader.GetDateTime(expiresAtOrdinal),
                RevokedAt = reader.IsDBNull(revokedAtOrdinal)
                    ? (DateTime?)null
                    : reader.GetDateTime(revokedAtOrdinal),
                DeviceInfo = reader.IsDBNull(deviceInfoOrdinal)
                    ? null
                    : reader.GetString(deviceInfoOrdinal)
            };

            return entity;
        }


        public void Revoke(RefreshTokenDbEntity token)
        {
            const string sql = @"
                        UPDATE user_refresh_tokens
                        SET revoked_at = @revoked_at
                        WHERE id = @id;";

            using var conn = CreateConnection();
            conn.Open();

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@revoked_at", token.RevokedAt ?? DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@id", token.Id);

            cmd.ExecuteNonQuery();
        }

        public void RevokeAllForUser(string userName)
        {
            const string sql = @"
                UPDATE user_refresh_tokens
                SET revoked_at = @revoked_at
                WHERE user_name = @user_name AND revoked_at IS NULL;";

            using var conn = CreateConnection();
            conn.Open();

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@revoked_at", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@user_name", userName);

            cmd.ExecuteNonQuery();
        }
    }

}
