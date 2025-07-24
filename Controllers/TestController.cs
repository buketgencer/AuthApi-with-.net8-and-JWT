using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly IJwtService _jwtService;

        public TestController(ILogger<TestController> logger, IJwtService jwtService)
        {
            _logger = logger;
            _jwtService = jwtService;
        }

        // JWT olmadan test
        [HttpGet("public")]
        public IActionResult PublicEndpoint()
        {
            return Ok(new { 
                message = "Bu endpoint herkese açık", 
                timestamp = DateTime.Now 
            });
        }

        // JWT ile test
        [HttpGet("protected")]
        [Authorize]
        public IActionResult ProtectedEndpoint()
        {
            // Tüm claim'leri logla
            _logger.LogInformation("=== USER CLAIMS ===");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation($"{claim.Type}: {claim.Value}");
            }

            var userId = User.FindFirst("user_id")?.Value;
            var username = User.FindFirst("username")?.Value;

            return Ok(new { 
                message = "Bu endpoint korumalı", 
                userId = userId,
                username = username,
                timestamp = DateTime.Now,
                allClaims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
        }

        // Token validation test
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] TokenRequest request)
        {
            try
            {
                _logger.LogInformation($"Validating token: {request.Token?.Substring(0, Math.Min(20, request.Token?.Length ?? 0))}...");
                
                var principal = _jwtService.ValidateToken(request.Token ?? "");
                if (principal != null)
                {
                    var claims = principal.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();
                    _logger.LogInformation("Token validation successful");
                    return Ok(new { valid = true, claims = claims });
                }
                
                _logger.LogWarning("Token validation failed");
                return BadRequest(new { valid = false, message = "Token geçersiz" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation error");
                return BadRequest(new { valid = false, message = ex.Message });
            }
        }

        // HTTP Headers debug
        [HttpGet("debug-headers")]
        [Authorize]
        public IActionResult DebugHeaders()
        {
            var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            
            _logger.LogInformation("=== REQUEST HEADERS ===");
            foreach (var header in headers)
            {
                _logger.LogInformation($"{header.Key}: {header.Value}");
            }

            return Ok(new {
                headers = headers,
                authorizationHeader = authHeader,
                hasAuthHeader = !string.IsNullOrEmpty(authHeader),
                bearerToken = authHeader?.StartsWith("Bearer ") == true ? 
                    authHeader.Substring(7, Math.Min(20, authHeader.Length - 7)) + "..." : "No Bearer token"
            });
        }
    }

    // Token request model
    public class TokenRequest
    {
        public string? Token { get; set; }
    }
}