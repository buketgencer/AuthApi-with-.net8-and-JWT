using System.ComponentModel.DataAnnotations;

namespace AuthApi.DTOs
{
    public class BmiRequest
    {
        [Required(ErrorMessage = "Boy zorunludur")]
        [Range(0.5, 3.0, ErrorMessage = "Boy 0.5 ile 3.0 metre arasında olmalıdır")]
        public double Height { get; set; } // metre cinsinden
        
        [Required(ErrorMessage = "Kilo zorunludur")]
        [Range(10, 500, ErrorMessage = "Kilo 10 ile 500 kg arasında olmalıdır")]
        public double Weight { get; set; } // kg cinsinden
        
        [Required(ErrorMessage = "Cinsiyet zorunludur")]
        [RegularExpression("^(Erkek|Kadın)$", ErrorMessage = "Cinsiyet 'Erkek' veya 'Kadın' olmalıdır")]
        public string Gender { get; set; } = string.Empty;
    }
}