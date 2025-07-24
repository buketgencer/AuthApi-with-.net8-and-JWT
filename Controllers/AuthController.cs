using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.DTOs;
using AuthApi.Services;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthDbContext context, IJwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcı kaydı
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            try
            {
                // Model doğrulaması
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Doğrulama hatası: {errors}"
                    });
                }

                // Email zaten kayıtlı mı kontrol et
                var existingUserByEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (existingUserByEmail != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Bu email adresi zaten kayıtlı."
                    });
                }

                // Kullanıcı adı zaten kayıtlı mı kontrol et
                var existingUserByUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

                if (existingUserByUsername != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Bu kullanıcı adı zaten kullanılıyor."
                    });
                }

                // Şifreyi hash'le
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // Rol kontrolü - eğer belirtilmemişse "User" olacak
                var userRole = UserRole.User; // Default role
                if (!string.IsNullOrEmpty(request.Role))
                {
                    if (UserRole.IsValidRole(request.Role))
                    {
                        userRole = request.Role;
                    }
                    else
                    {
                        return BadRequest(new AuthResponse
                        {
                            Success = false,
                            Message = $"Geçersiz rol. Geçerli roller: {string.Join(", ", UserRole.GetAllRoles())}"
                        });
                    }
                }

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Role = userRole,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Yeni kullanıcı kaydedildi: {user.Username}");

                // JWT token oluştur
                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Kullanıcı başarıyla kaydedildi.",
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında hata oluştu");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// Kullanıcı girişi
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            try
            {
                // Model doğrulaması
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = $"Doğrulama hatası: {errors}"
                    });
                }

                // Kullanıcıyı bul
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower());

                if (user == null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Email veya şifre hatalı."
                    });
                }

                // Kullanıcı aktif mi kontrol et
                if (!user.IsActive)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Hesabınız devre dışı bırakılmış."
                    });
                }

                // Şifreyi doğrula
                if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Email veya şifre hatalı."
                    });
                }

                // Son giriş tarihini güncelle
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // JWT token oluştur
                var token = _jwtService.GenerateToken(user);
                var expiresAt = DateTime.UtcNow.AddHours(24);

                _logger.LogInformation($"Kullanıcı giriş yaptı: {user.Username}");

                // Role göre hoşgeldin mesajı
                var welcomeMessage = user.Role == UserRole.Admin
                    ? $"Hoşgeldin Admin {user.Username}!"
                    : $"Hoşgeldin {user.Username}!";

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = welcomeMessage,
                    Token = token,
                    ExpiresAt = expiresAt,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı girişi sırasında hata oluştu");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// Kullanıcı profili - Token gerektirir
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<AuthResponse>> GetProfile()
        {
            try
            {
                // JWT token'dan kullanıcı ID'sini al
                var userIdClaim = User.FindFirst("user_id")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Geçersiz token."
                    });
                }

                // Kullanıcıyı veritabanından bul
                var user = await _context.Users.FindAsync(userId);

                if (user == null || !user.IsActive)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı."
                    });
                }

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Profil bilgileri alındı.",
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt,
                        LastLoginAt = user.LastLoginAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil bilgileri alınırken hata oluştu");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// Test endpoint - Herkese açık
        /// </summary>
        [HttpGet("test")]
        public ActionResult<object> Test()
        {
            return Ok(new
            {
                Success = true,
                Message = "AuthApi çalışıyor!",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// Admin panel - Sadece Admin rolü
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<object>> AdminPanel()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var adminUsers = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);

                var recentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.Role,
                        u.CreatedAt,
                        u.LastLoginAt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Message = "Admin panel verisi",
                    Data = new
                    {
                        Statistics = new
                        {
                            TotalUsers = totalUsers,
                            ActiveUsers = activeUsers,
                            AdminUsers = adminUsers,
                            UserUsers = totalUsers - adminUsers
                        },
                        RecentUsers = recentUsers
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin panel verisi alınırken hata oluştu");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }
    }
    
    
}