using GroupHomework66.Enums;
using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class VacancyService : IVacancyService
{
    private const int PageSize = 20;
    private readonly SiteMobileContext _context;

    public VacancyService(SiteMobileContext context)
    {
        _context = context;
    }

    public async Task<List<Vacancy>> GetEmployerVacanciesAsync(int employerId)
    {
        return await _context.Vacancies
            .AsNoTracking()
            .Where(v => v.EmployerId == employerId)
            .OrderByDescending(v => v.UpdatedDate)
            .ToListAsync();
    }

    public async Task<Vacancy?> GetEmployerVacancyAsync(int employerId, int vacancyId)
    {
        return await _context.Vacancies
            .FirstOrDefaultAsync(v => v.Id == vacancyId && v.EmployerId == employerId);
    }

    public async Task<ApplicantVacancyDetails?> GetApplicantVacancyDetailsAsync(int applicantId, int vacancyId)
    {
        var vacancy = await _context.Vacancies
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == vacancyId && v.IsPublished);
        if (vacancy == null)
        {
            return null;
        }

        var resumes = await _context.Resumes
            .Where(r => r.UserId == applicantId)
            .Select(r => new ResumeOption(r.Id, r.Title))
            .ToListAsync();

        var resumeIds = resumes.Select(r => r.Id).ToList();
        vacancy.IsRespondedByTheCurrentUser = await _context.Responses
            .AnyAsync(r => r.VacancyId == vacancyId && resumeIds.Contains(r.ResumeId));

        return new ApplicantVacancyDetails { Vacancy = vacancy, Resumes = resumes };
    }

    public async Task<VacancyListResult> GetPublishedListAsync(string? search, VacancyCategory? category,
        string? sortBySalary, int page, int? applicantId)
    {
        if (page < 1)
        {
            page = 1;
        }

        var query = _context.Vacancies
            .AsNoTracking()
            .Where(v => v.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(v => v.Title.ToLower().Contains(term));
        }

        if (category.HasValue)
        {
            query = query.Where(v => v.Category == category.Value);
        }

        query = sortBySalary switch
        {
            "asc" => query.OrderBy(v => v.Salary),
            "desc" => query.OrderByDescending(v => v.Salary),
            _ => query.OrderByDescending(v => v.UpdatedDate)
        };

        var totalItems = await query.CountAsync();
        var vacancies = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var result = new VacancyListResult
        {
            Vacancies = vacancies,
            Page = page,
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize)
        };

        if (applicantId.HasValue)
        {
            result.Resumes = await _context.Resumes
                .Where(r => r.UserId == applicantId.Value)
                .Select(r => new ResumeOption(r.Id, r.Title))
                .ToListAsync();

            var resumeIds = result.Resumes.Select(r => r.Id).ToList();
            var respondedIds = await _context.Responses
                .Where(r => resumeIds.Contains(r.ResumeId))
                .Select(r => r.VacancyId)
                .ToListAsync();

            foreach (var v in vacancies)
            {
                v.IsRespondedByTheCurrentUser = respondedIds.Contains(v.Id);
            }
        }

        return result;
    }

    public async Task CreateAsync(int employerId, CreateVacancyViewModel model)
    {
        var now = DateTime.UtcNow;
        _context.Vacancies.Add(new Vacancy
        {
            EmployerId = employerId,
            Title = model.Title.Trim(),
            Salary = model.Salary!.Value,
            Description = model.Description.Trim(),
            ExperienceFrom = model.ExperienceFrom,
            ExperienceTo = model.ExperienceTo,
            Category = model.Category!.Value,
            CreatedDate = now,
            UpdatedDate = now,
            IsPublished = true
        });
        await _context.SaveChangesAsync();
    }

    public async Task<CreateVacancyViewModel?> GetForEditAsync(int employerId, int vacancyId)
    {
        var vacancy = await GetEmployerVacancyAsync(employerId, vacancyId);
        if (vacancy == null)
        {
            return null;
        }

        return new CreateVacancyViewModel
        {
            Title = vacancy.Title,
            Salary = vacancy.Salary,
            Description = vacancy.Description,
            ExperienceFrom = vacancy.ExperienceFrom,
            ExperienceTo = vacancy.ExperienceTo,
            Category = vacancy.Category
        };
    }

    public async Task<bool> UpdateAsync(int employerId, int vacancyId, CreateVacancyViewModel model)
    {
        var vacancy = await GetEmployerVacancyAsync(employerId, vacancyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.Title = model.Title.Trim();
        vacancy.Salary = model.Salary!.Value;
        vacancy.Description = model.Description.Trim();
        vacancy.ExperienceFrom = model.ExperienceFrom;
        vacancy.ExperienceTo = model.ExperienceTo;
        vacancy.Category = model.Category!.Value;
        vacancy.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefreshAsync(int employerId, int vacancyId)
    {
        var vacancy = await GetEmployerVacancyAsync(employerId, vacancyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TogglePublishAsync(int employerId, int vacancyId)
    {
        var vacancy = await GetEmployerVacancyAsync(employerId, vacancyId);
        if (vacancy == null)
        {
            return false;
        }

        vacancy.IsPublished = !vacancy.IsPublished;
        await _context.SaveChangesAsync();
        return true;
    }
}