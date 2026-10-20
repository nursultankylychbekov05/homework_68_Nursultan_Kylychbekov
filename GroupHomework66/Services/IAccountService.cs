using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface IAccountService
{
    Task<ServiceResult> RegisterAsync(RegisterViewModel model);
    Task<ServiceResult> LoginAsync(LoginViewModel model);
    Task LogOffAsync();

    Task<bool> IsEmailAvailableAsync(string email);
    Task<bool> IsUserNameAvailableAsync(string userName);
}