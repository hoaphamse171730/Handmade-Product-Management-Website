namespace HandmadeProductManagement.Core.Utils
{
    public static class TimeHelper
    {
        public static DateTime ConvertToUtcPlus7(DateTime dateTime)
        {
            // UTC+7 is 7 hours ahead of UTC
            TimeSpan utcPlus7Offset = new(7, 0, 0);
            return dateTime.ToUniversalTime().Add(utcPlus7Offset);
        }

        public static DateTime ConvertToUtcPlus7NotChanges(DateTime dateTime)
        {
            // UTC+7 is 7 hours ahead of UTC
            TimeSpan utcPlus7Offset = new(7, 0, 0);
            return dateTime.ToUniversalTime().Add(utcPlus7Offset).AddHours(-7);
        }
    }
}
