namespace HandmadeProductManagement.ModelViews.UserInfoModelViews
{
    public class UserInfoDto
    {
        public string Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; }
        public string? DisplayName { get; set; } = default!;
        public string? Bio { get; set; } = default!;
        public string? BankAccount { get; set; }
        public string? BankAccountName { get; set; }
        public string? Bank { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
