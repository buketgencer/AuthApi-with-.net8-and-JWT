using System.ComponentModel.DataAnnotations;

namespace AuthApi.Models
{
    public class BmiData
    {
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [Range(0.5, 3.0, ErrorMessage = "Boy 0.5 ile 3.0 metre arasında olmalıdır")]
        public double Height { get; set; } // metre cinsinden
        
        [Required]
        [Range(10, 500, ErrorMessage = "Kilo 10 ile 500 kg arasında olmalıdır")]
        public double Weight { get; set; } // kg cinsinden
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // "Erkek" veya "Kadın"
        
        public double BmiValue { get; set; } // Hesaplanan BMI değeri
        
        public string BmiCategory { get; set; } = string.Empty; // BMI kategorisi
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property
        public User User { get; set; } = null!;
    }
    
    public static class BmiCalculator
    {
        public static double CalculateBmi(double height, double weight)
        {
            return Math.Round(weight / (height * height), 2);
        }
        
        public static string GetBmiCategory(double bmiValue)
        {
            return bmiValue switch
            {
                < 18.5 => "Zayıf",
                >= 18.5 and < 25 => "Normal",
                >= 25 and < 30 => "Fazla Kilolu",
                >= 30 and < 35 => "Obez (1. Derece)",
                >= 35 and < 40 => "Obez (2. Derece)",
                >= 40 => "Morbid Obez",
                _ => "Bilinmiyor"
            };
        }
        
        public static string GetBmiAdvice(double bmiValue, string gender)
        {
            return bmiValue switch
            {
                < 18.5 => "Kilo almanız önerilir. Dengeli beslenme ve düzenli egzersiz yapın.",
                >= 18.5 and < 25 => "İdeal kilodasınız! Bu durumu korumaya devam edin.",
                >= 25 and < 30 => "Fazla kilolu kategorisinde olmakta. Sağlıklı beslenme ve egzersiz önerilir.",
                >= 30 and < 35 => "Obezite riski var. Bir uzman ile görüşmeniz önerilir.",
                >= 35 and < 40 => "Ciddi obezite riski. Mutlaka bir sağlık uzmanına danışın.",
                >= 40 => "Morbid obezite. Acil tıbbi müdahale gerekebilir.",
                _ => "Bilinmiyor"
            };
        }
    }
}