namespace HandmadeProductManagement.Core.Common;

public static class CustomRegex
{
    public const string UsernameRegex = @"^[A-Za-z]+[A-Za-z0-9.]*$";
    public const string FullNameRegex = @"^[ A-Za-z]+$";
    public const string SpecialCharacterRegex = @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]";
    
}