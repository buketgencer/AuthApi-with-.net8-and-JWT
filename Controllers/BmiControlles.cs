using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AuthApi.Data;
using AuthApi.Models;
using AuthApi.DTOs;

namespace AuthApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Tüm endpoint'ler JWT token gerektirir
    public class BmiController : ControllerBase
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<BmiController> _logger;

        public BmiController(AuthDbContext context, ILogger<BmiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// BMI hesaplama ve kaydetme (Tüm kullanıcılar için)
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<BmiResponse>> CalculateBmi(BmiRequest request)
        {
            try
            {
                // Model doğrulaması
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));

                    return BadRequest(new BmiResponse
                    {
                        Success = false,
                        Message = $"Doğrulama hatası: {errors}"
                    });
                }

                // JWT token'dan kullanıcı ID'sini al
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BmiResponse
                    {
                        Success = false,
                        Message = "Geçersiz token."
                    });
                }

                // Kullanıcıyı kontrol et
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.IsActive)
                {
                    return NotFound(new BmiResponse
                    {
                        Success = false,
                        Message = "Kullanıcı bulunamadı."
                    });
                }

                // BMI hesapla
                var bmiValue = BmiCalculator.CalculateBmi(request.Height, request.Weight);
                var bmiCategory = BmiCalculator.GetBmiCategory(bmiValue);
                var advice = BmiCalculator.GetBmiAdvice(bmiValue, request.Gender);

                // Kullanıcının mevcut BMI kaydını kontrol et
                var existingBmi = await _context.BmiData
                    .FirstOrDefaultAsync(b => b.UserId == userId);

                if (existingBmi != null)
                {
                    // Mevcut kaydı güncelle
                    existingBmi.Height = request.Height;
                    existingBmi.Weight = request.Weight;
                    existingBmi.Gender = request.Gender;
                    existingBmi.BmiValue = bmiValue;
                    existingBmi.BmiCategory = bmiCategory;
                    existingBmi.UpdatedAt = DateTime.UtcNow;

                    _context.BmiData.Update(existingBmi);
                }
                else
                {
                    // Yeni kayıt oluştur
                    var bmiData = new BmiData
                    {
                        UserId = userId,
                        Height = request.Height,
                        Weight = request.Weight,
                        Gender = request.Gender,
                        BmiValue = bmiValue,
                        BmiCategory = bmiCategory,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.BmiData.Add(bmiData);
                    existingBmi = bmiData;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"BMI hesaplandı - Kullanıcı: {user.Username}, BMI: {bmiValue}");

                return Ok(new BmiResponse
                {
                    Success = true,
                    Message = "BMI başarıyla hesaplandı ve kaydedildi.",
                    Data = new BmiResult
                    {
                        Id = existingBmi.Id,
                        Height = existingBmi.Height,
                        Weight = existingBmi.Weight,
                        Gender = existingBmi.Gender,
                        BmiValue = existingBmi.BmiValue,
                        BmiCategory = existingBmi.BmiCategory,
                        Advice = advice,
                        CreatedAt = existingBmi.CreatedAt,
                        UpdatedAt = existingBmi.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BMI hesaplama sırasında hata oluştu");
                return StatusCode(500, new BmiResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// Kullanıcının BMI bilgisini getir
        /// </summary>
        [HttpGet("my-bmi")]
        public async Task<ActionResult<BmiResponse>> GetMyBmi()
        {
            try
            {
                // JWT token'dan kullanıcı ID'sini al
                var userIdClaim = User.FindFirst("user_id")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new BmiResponse
                    {
                        Success = false,
                        Message = "Geçersiz token."
                    });
                }

                // BMI kaydını bul
                var bmiData = await _context.BmiData
                    .FirstOrDefaultAsync(b => b.UserId == userId);

                if (bmiData == null)
                {
                    return NotFound(new BmiResponse
                    {
                        Success = false,
                        Message = "BMI kaydı bulunamadı. Lütfen önce BMI hesaplaması yapın."
                    });
                }

                var advice = BmiCalculator.GetBmiAdvice(bmiData.BmiValue, bmiData.Gender);

                return Ok(new BmiResponse
                {
                    Success = true,
                    Message = "BMI bilgileri getirildi.",
                    Data = new BmiResult
                    {
                        Id = bmiData.Id,
                        Height = bmiData.Height,
                        Weight = bmiData.Weight,
                        Gender = bmiData.Gender,
                        BmiValue = bmiData.BmiValue,
                        BmiCategory = bmiData.BmiCategory,
                        Advice = advice,
                        CreatedAt = bmiData.CreatedAt,
                        UpdatedAt = bmiData.UpdatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BMI bilgileri getirilirken hata oluştu");
                return StatusCode(500, new BmiResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// Tüm kullanıcıların BMI bilgilerini getir - Sadece Admin
        /// </summary>
        [HttpGet("all-users-bmi")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<AdminBmiListResponse>> GetAllUsersBmi()
        {
            try
            {
                var bmiDataList = await _context.BmiData
                    .Include(b => b.User)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new AdminBmiResult
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        Username = b.User.Username,
                        Email = b.User.Email,
                        Height = b.Height,
                        Weight = b.Weight,
                        Gender = b.Gender,
                        BmiValue = b.BmiValue,
                        BmiCategory = b.BmiCategory,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt
                    })
                    .ToListAsync();

                return Ok(new AdminBmiListResponse
                {
                    Success = true,
                    Message = $"Toplam {bmiDataList.Count} kullanıcının BMI bilgileri getirildi.",
                    Data = bmiDataList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Tüm kullanıcıların BMI bilgileri getirilirken hata oluştu");
                return StatusCode(500, new AdminBmiListResponse
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }

        /// <summary>
        /// BMI istatistikleri - Sadece Admin
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<ActionResult<object>> GetBmiStatistics()
        {
            try
            {
                var totalBmiRecords = await _context.BmiData.CountAsync();
                var maleCount = await _context.BmiData.CountAsync(b => b.Gender == "Erkek");
                var femaleCount = await _context.BmiData.CountAsync(b => b.Gender == "Kadın");

                var categoryStats = await _context.BmiData
                    .GroupBy(b => b.BmiCategory)
                    .Select(g => new
                    {
                        Category = g.Key,
                        Count = g.Count(),
                        Percentage = Math.Round((double)g.Count() / totalBmiRecords * 100, 2)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var averageBmi = await _context.BmiData.AverageAsync(b => b.BmiValue);

                return Ok(new
                {
                    Success = true,
                    Message = "BMI istatistikleri getirildi.",
                    Data = new
                    {
                        TotalRecords = totalBmiRecords,
                        GenderDistribution = new
                        {
                            Male = maleCount,
                            Female = femaleCount,
                            MalePercentage = totalBmiRecords > 0 ? Math.Round((double)maleCount / totalBmiRecords * 100, 2) : 0,
                            FemalePercentage = totalBmiRecords > 0 ? Math.Round((double)femaleCount / totalBmiRecords * 100, 2) : 0
                        },
                        CategoryDistribution = categoryStats,
                        AverageBmi = Math.Round(averageBmi, 2)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "BMI istatistikleri getirilirken hata oluştu");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Sunucu hatası oluştu."
                });
            }
        }
    }
}