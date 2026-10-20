using GroupHomework66.Enums;
using GroupHomework66.Models;
using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface IVacancyService
{
    Task<List<Vacancy>> GetEmployerVacanciesAsync(int employerId);
    Task<Vacancy?> GetEmployerVacancyAsync(int employerId, int vacancyId);
    Task<ApplicantVacancyDetails?> GetApplicantVacancyDetailsAsync(int applicantId, int vacancyId);
    Task<VacancyListResult> GetPublishedListAsync(string? search, VacancyCategory? category,
        string? sortBySalary, int page, int? applicantId);

    Task CreateAsync(int employerId, CreateVacancyViewModel model);
    Task<CreateVacancyViewModel?> GetForEditAsync(int employerId, int vacancyId);
    Task<bool> UpdateAsync(int employerId, int vacancyId, CreateVacancyViewModel model);
    Task<bool> RefreshAsync(int employerId, int vacancyId);
    Task<bool> TogglePublishAsync(int employerId, int vacancyId);
}