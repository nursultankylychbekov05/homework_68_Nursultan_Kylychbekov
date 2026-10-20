using GroupHomework66.Enums;
using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class AccountService : IAccountService
{
    private const string DefaultPicture = "https://dme.lk/images/user.jpg";

    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ServiceResult> RegisterAsync(RegisterViewModel model)
    {
        var user = new User
        {
            Email = model.Email,
            UserName = model.Username,
            PhoneNumber = model.PhoneNumber,
            ProfilePicture = string.IsNullOrEmpty(model.ProfilePicture) ? DefaultPicture : model.ProfilePicture
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));
        }

        if (model.Role == ChosenRole.Applicant)
        {
            await _userManager.AddToRoleAsync(user, "Applicant");
        }
        else if (model.Role == ChosenRole.Company)
        {
            await _userManager.AddToRoleAsync(user, "Company");
        }

        await _signInManager.SignInAsync(user, false);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> LoginAsync(LoginViewModel model)
    {
        const string error = "Неправильный логин и (или) пароль";

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            return ServiceResult.Failure(new[] { error });
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!, model.Password, model.RememberMe, false);

        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(new[] { error });
    }

    public async Task LogOffAsync()
    {
        await _signInManager.SignOutAsync();
    }

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        return !await _userManager.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUserNameAvailableAsync(string userName)
    {
        return !await _userManager.Users.AnyAsync(u => u.UserName == userName);
    }
}