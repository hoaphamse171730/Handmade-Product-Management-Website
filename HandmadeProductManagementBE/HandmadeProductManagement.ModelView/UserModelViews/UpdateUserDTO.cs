namespace HandmadeProductManagement.ModelViews.UserModelViews
{
    public  class UpdateUserDTO
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
        
    }
}
