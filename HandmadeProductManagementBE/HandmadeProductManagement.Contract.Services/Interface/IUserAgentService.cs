namespace HandmadeProductManagement.Contract.Services.Interface;

public interface IUserAgentService
{
    (string operatingSystem, string browser) GetClientInfo();
}