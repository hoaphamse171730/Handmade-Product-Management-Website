using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Core.Utils;

public static class ValidationHelper
{
    public static bool IsValidNames(params string?[] credentials)
    {
        var validRegex = @"^[A-Za-z]+[A-Za-z0-9 ]*$";
        foreach (var credential in credentials)
        {
            if (credential is null ||
                credential.TrimStart().Length != credential.Length ||
                credential.TrimEnd().Length != credential.Length ||
                !Regex.IsMatch(credential, validRegex)
               )
                return false;
        }
        return true;
    }
}