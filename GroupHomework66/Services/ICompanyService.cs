using GroupHomework66.Enums;
using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface ICompanyService
{
    Task<CompanyProfileViewModel?> GetProfileAsync(int userId, EditCompanyProfileViewModel? edit = null);
    Task<ServiceResult> UpdateProfileAsync(int userId, EditCompanyProfileViewModel model);
    Task<ResumesCatalogResult> GetResumesCatalogAsync(int employerId, VacancyCategory? category);
}