namespace AuthApi.Models
{
    public static class UserRole
    {
        public const string Admin = "Admin";
        public const string User = "User";
        
        public static string[] GetAllRoles()
        {
            return new[] { Admin, User };
        }
        
        public static bool IsValidRole(string role)
        {
            return GetAllRoles().Contains(role);
        }
    }
}