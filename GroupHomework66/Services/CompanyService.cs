using GroupHomework66.Enums;
using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class CompanyService : ICompanyService
{
    private const string DefaultPicture = "https://dme.lk/images/user.jpg";

    private readonly SiteMobileContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IVacancyService _vacancyService;

    public CompanyService(SiteMobileContext context, UserManager<User> userManager,
        SignInManager<User> signInManager, IVacancyService vacancyService)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _vacancyService = vacancyService;
    }

    public async Task<CompanyProfileViewModel?> GetProfileAsync(int userId, EditCompanyProfileViewModel? edit = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        return new CompanyProfileViewModel
        {
            CompanyName = user.UserName ?? string.Empty,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ProfilePicture = user.ProfilePicture,
            Vacancies = await _vacancyService.GetEmployerVacanciesAsync(userId),
            Edit = edit ?? new EditCompanyProfileViewModel
            {
                CompanyName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                ProfilePicture = user.ProfilePicture
            }
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(int userId, EditCompanyProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ServiceResult.Failure(new[] { "Пользователь не найден." });
        }

        user.UserName = model.CompanyName.Trim();
        user.Email = model.Email.Trim();
        user.PhoneNumber = model.PhoneNumber.Trim();
        user.ProfilePicture = string.IsNullOrWhiteSpace(model.ProfilePicture)
            ? DefaultPicture
            : model.ProfilePicture.Trim();

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult.Failure(result.Errors.Select(e => e.Description));
        }
        
        await _signInManager.RefreshSignInAsync(user);
        return ServiceResult.Success();
    }

    public async Task<ResumesCatalogResult> GetResumesCatalogAsync(int employerId, VacancyCategory? category)
    {
        var query = _context.Resumes
            .AsNoTracking()
            .Where(r => r.IsPublished)
            .Include(r => r.User)
            .Include(r => r.WorkExperiences)
            .AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(r => r.Category == category.Value);
        }

        return new ResumesCatalogResult
        {
            Resumes = await query.ToListAsync(),
            EmployerVacancies = (await _vacancyService.GetEmployerVacanciesAsync(employerId))
                .Where(v => v.IsPublished)
                .ToList(),
            SelectedCategory = category
        };
    }
}