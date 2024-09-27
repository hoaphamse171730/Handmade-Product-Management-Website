namespace HandmadeProductManagement.Core.Utils;

public static class UsernameHelper
{
    private static readonly Random _random = new Random();

    public static string GenerateUsername(string fullName)
    {
        //Use the first letter of the first name and up to 5 characters of the last name
        string baseUsername = $"{fullName[0]}{fullName.Substring(1, Math.Min(5, fullName.Length))}".ToLower();
        
        //Add a random 4-digit number for uniqueness
        string randomNumber = _random.Next(1000, 9999).ToString();
        
        //Combine base username with the random number
        return $"{baseUsername}{randomNumber}";
    }
}