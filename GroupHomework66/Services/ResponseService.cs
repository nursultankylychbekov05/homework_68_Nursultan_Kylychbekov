using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class ResponseService : IResponseService
{
    private readonly SiteMobileContext _context;

    public ResponseService(SiteMobileContext context)
    {
        _context = context;
    }

    public async Task<ResponseOperationResult> CreateAsync(int applicantId, int vacancyId, int resumeId)
    {
        var ownsResume = await _context.Resumes.AnyAsync(r => r.Id == resumeId && r.UserId == applicantId);
        if (!ownsResume)
        {
            return ResponseOperationResult.Fail("Резюме не найдено или не принадлежит вам.");
        }

        var vacancyAvailable = await _context.Vacancies.AnyAsync(v => v.Id == vacancyId && v.IsPublished);
        if (!vacancyAvailable)
        {
            return ResponseOperationResult.Missing("Вакансия не найдена.");
        }

        var userResumeIds = await _context.Resumes
            .Where(r => r.UserId == applicantId)
            .Select(r => r.Id)
            .ToListAsync();

        var exists = await _context.Responses
            .AnyAsync(r => r.VacancyId == vacancyId && userResumeIds.Contains(r.ResumeId));
        if (exists)
        {
            return ResponseOperationResult.Fail("Вы уже откликнулись на эту вакансию.");
        }

        _context.Responses.Add(new Response
        {
            VacancyId = vacancyId,
            ResumeId = resumeId,
            ResponseDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return ResponseOperationResult.Ok();
    }

    public async Task<ResponseOperationResult> RemoveAsync(int applicantId, int vacancyId)
    {
        var userResumeIds = await _context.Resumes
            .Where(r => r.UserId == applicantId)
            .Select(r => r.Id)
            .ToListAsync();

        if (!userResumeIds.Any())
        {
            return ResponseOperationResult.Missing("У вас нет резюме.");
        }

        var response = await _context.Responses
            .FirstOrDefaultAsync(r => r.VacancyId == vacancyId && userResumeIds.Contains(r.ResumeId));
        if (response == null)
        {
            return ResponseOperationResult.Missing("Отклик не найден.");
        }

        _context.Responses.Remove(response);
        await _context.SaveChangesAsync();

        return ResponseOperationResult.Ok();
    }

    public async Task<List<CompanyResponseDto>> GetCompanyResponsesAsync(int employerId)
    {
        return await _context.Responses
            .AsNoTracking()
            .Where(r => r.Vacancy != null && r.Vacancy.EmployerId == employerId)
            .Select(r => new CompanyResponseDto(
                r.Id,
                r.ResponseDate,
                r.VacancyId,
                r.Vacancy != null ? r.Vacancy.Title : null,
                r.ResumeId))
            .ToListAsync();
    }

    public async Task<List<ApplicantResponseDto>> GetApplicantResponsesAsync(int applicantId)
    {
        return await _context.Responses
            .AsNoTracking()
            .Where(r => r.Resume != null && r.Resume.UserId == applicantId)
            .Select(r => new ApplicantResponseDto(
                r.Id,
                r.ResponseDate,
                r.ResumeId,
                r.Resume != null ? r.Resume.Title : null,
                r.VacancyId,
                r.Vacancy != null ? r.Vacancy.Title : null))
            .ToListAsync();
    }

    public async Task<CompanyResponsesViewModel> GetCompanyResponsesPageAsync(int employerId)
    {
        var vacancies = await _context.Vacancies
            .AsNoTracking()
            .Where(v => v.EmployerId == employerId)
            .OrderByDescending(v => v.UpdatedDate)
            .ToListAsync();

        var responses = await _context.Responses
            .AsNoTracking()
            .Where(r => r.Vacancy!.EmployerId == employerId)
            .Include(r => r.Resume).ThenInclude(r => r!.User)
            .OrderByDescending(r => r.ResponseDate)
            .ToListAsync();

        return new CompanyResponsesViewModel
        {
            Vacancies = vacancies.Select(v => new VacancyResponsesGroup
            {
                VacancyId = v.Id,
                VacancyTitle = v.Title,
                IsPublished = v.IsPublished,
                Responses = responses
                    .Where(r => r.VacancyId == v.Id)
                    .Select(r => new ResponseItem
                    {
                        ResponseId = r.Id,
                        ApplicantId = r.Resume?.UserId ?? 0,
                        ResumeTitle = r.Resume?.Title ?? "Резюме удалено",
                        ApplicantName = r.Resume?.User?.UserName ?? "Неизвестно",
                        ResponseDate = r.ResponseDate
                    })
                    .ToList()
            }).ToList()
        };
    }

    public async Task<Response?> GetResponseForEmployerAsync(int employerId, int responseId)
    {
        var response = await _context.Responses
            .AsNoTracking()
            .Include(r => r.Vacancy)
            .Include(r => r.Resume).ThenInclude(r => r!.User)
            .Include(r => r.Resume).ThenInclude(r => r!.WorkExperiences)
            .Include(r => r.Resume).ThenInclude(r => r!.Educations)
            .Include(r => r.Resume).ThenInclude(r => r!.Courses)
            .FirstOrDefaultAsync(r => r.Id == responseId && r.Vacancy!.EmployerId == employerId);

        return response?.Resume == null ? null : response;
    }

    public async Task<List<Response>> GetApplicantResponsesWithDetailsAsync(int applicantId)
    {
        return await _context.Responses
            .AsNoTracking()
            .Where(r => r.Resume != null && r.Resume.UserId == applicantId)
            .Include(r => r.Resume)
            .Include(r => r.Vacancy).ThenInclude(v => v!.Employer)
            .OrderByDescending(r => r.ResponseDate)
            .ToListAsync();
    }
}