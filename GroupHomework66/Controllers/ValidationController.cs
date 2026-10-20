using GroupHomework66.Services;
using Microsoft.AspNetCore.Mvc;

namespace GroupHomework66.Controllers;

public class ValidationController : Controller
{
    private readonly IAccountService _accountService;

    public ValidationController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public bool ValidatePhoneNumber(string PhoneNumber) => PhoneNumber.All(char.IsDigit);

    public Task<bool> ValidateEmailOnRegistration(string Email) => _accountService.IsEmailAvailableAsync(Email);

    public Task<bool> ValidateUserNameOnRegistration(string UserName) => _accountService.IsUserNameAvailableAsync(UserName);

    public bool ValidatePassword(string Password) =>
        Password.Length >= 8 && Password.Any(char.IsDigit) && Password.Any(char.IsUpper) && Password.Any(c => !char.IsLetter(c));

    public bool ValidateDateOfBirth(DateTime DateOfBirth) => DateOfBirth <= DateTime.UtcNow.AddYears(-18);
}