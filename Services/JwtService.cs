using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using AuthApi.Models;



namespace AuthApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            try
            {
                var keyString = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(keyString))
                {
                    throw new InvalidOperationException("JWT key is not configured.");
                }

                var key = Encoding.UTF8.GetBytes(keyString);
                var tokenHandler = new JwtSecurityTokenHandler();

                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];
                var expiryHoursStr = _configuration["Jwt:ExpiryHours"];

                _logger.LogInformation("=== TOKEN GENERATION DEBUG ===");
                _logger.LogInformation("Key length: {KeyLength} bytes", key.Length);
                _logger.LogInformation("Issuer: '{Issuer}'", issuer);
                _logger.LogInformation("Audience: '{Audience}'", audience);
                _logger.LogInformation("Expiry Hours String: '{ExpiryHoursStr}'", expiryHoursStr);

                if (!double.TryParse(expiryHoursStr, out double expiryHours))
                {
                    expiryHours = 24; // Default expiry time
                    _logger.LogWarning("Could not parse ExpiryHours '{ExpiryHoursStr}', using default: {ExpiryHours}", expiryHoursStr, expiryHours);
                }

                var now = DateTime.UtcNow;
                var expiry = now.AddHours(expiryHours);

                _logger.LogInformation("Current UTC time: {Now}", now);
                _logger.LogInformation("Token will expire at: {Expiry}", expiry);
                _logger.LogInformation("Expiry hours: {ExpiryHours}", expiryHours);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("user_id", user.Id.ToString()),
                    new Claim("username", user.Username),
                    new Claim("email", user.Email),
                    new Claim("role", user.Role),

                    // Standard JWT claims
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiry).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Iss, issuer ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Aud, audience ?? string.Empty)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expiry,
                    NotBefore = now,
                    IssuedAt = now,
                    Issuer = issuer,
                    Audience = audience,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("Token generated successfully for user: {Username}", user.Username);
                _logger.LogInformation("Token length: {TokenLength}", tokenString.Length);

                // Optional: read token back to log details
                var readToken = tokenHandler.ReadJwtToken(tokenString);
                _logger.LogInformation("Generated token issuer: '{Issuer}'", readToken.Issuer);
                _logger.LogInformation("Generated token audiences: [{Audiences}]", string.Join(", ", readToken.Audiences));
                _logger.LogInformation("Generated token expires: {Expires}", readToken.ValidTo);

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token");
                throw;
            }
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("Token is null or empty");
                    return null;
                }

                _logger.LogInformation("=== TOKEN VALIDATION DEBUG ===");
                _logger.LogInformation("Token length: {TokenLength}", token.Length);

                var keyString = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(keyString))
                {
                    _logger.LogError("JWT key is not configured.");
                    return null;
                }

                var key = Encoding.UTF8.GetBytes(keyString);
                var tokenHandler = new JwtSecurityTokenHandler();

                if (!tokenHandler.CanReadToken(token))
                {
                    _logger.LogWarning("Token format is invalid - cannot read token");
                    return null;
                }

                var jwtToken = tokenHandler.ReadJwtToken(token);
                _logger.LogInformation("Token issuer: '{Issuer}'", jwtToken.Issuer);
                _logger.LogInformation("Token audiences: [{Audiences}]", string.Join(", ", jwtToken.Audiences));
                _logger.LogInformation("Token expires: {Expires}", jwtToken.ValidTo);
                _logger.LogInformation("Current UTC time: {Now}", DateTime.UtcNow);
                _logger.LogInformation("Token expired: {IsExpired}", jwtToken.ValidTo < DateTime.UtcNow);

                var configIssuer = _configuration["Jwt:Issuer"];
                var configAudience = _configuration["Jwt:Audience"];

                _logger.LogInformation("Config Issuer: '{Issuer}'", configIssuer);
                _logger.LogInformation("Config Audience: '{Audience}'", configAudience);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = configIssuer,
                    ValidateAudience = true,
                    ValidAudience = configAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateActor = false,
                    ValidateTokenReplay = false
                };

                _logger.LogInformation("Starting token validation...");
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                _logger.LogInformation("Token validation successful!");
                return principal;
            }
            catch (SecurityTokenExpiredException ex)
            {
                _logger.LogWarning("Token expired: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidSignatureException ex)
            {
                _logger.LogWarning("Invalid token signature: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                _logger.LogWarning("Invalid issuer: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenInvalidAudienceException ex)
            {
                _logger.LogWarning("Invalid audience: {Message}", ex.Message);
                return null;
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogWarning("Token validation failed: {Message}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token validation");
                return null;
            }
        }
    }
}
