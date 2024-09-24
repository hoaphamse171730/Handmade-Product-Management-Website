namespace HandmadeProductManagement.Core.Utils
{
    public class CoreHelper
    {
        public static DateTime SystemTimeNow => TimeHelper.ConvertToUtcPlus7(DateTime.Now);
    }
}
