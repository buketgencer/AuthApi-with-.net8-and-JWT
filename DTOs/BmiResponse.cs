namespace AuthApi.DTOs
{
    public class BmiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public BmiResult? Data { get; set; }
    }
    
    public class BmiResult
    {
        public int Id { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Gender { get; set; } = string.Empty;
        public double BmiValue { get; set; }
        public string BmiCategory { get; set; } = string.Empty;
        public string Advice { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    public class AdminBmiListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AdminBmiResult>? Data { get; set; }
    }
    
    public class AdminBmiResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Gender { get; set; } = string.Empty;
        public double BmiValue { get; set; }
        public string BmiCategory { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}