using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class CandidateService : ICandidateService
{
    private const string DefaultPicture = "https://dme.lk/images/user.jpg";

    private readonly SiteMobileContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ResumePdfService _resumePdfService;

    public CandidateService(SiteMobileContext context, UserManager<User> userManager,
        ResumePdfService resumePdfService)
    {
        _context = context;
        _userManager = userManager;
        _resumePdfService = resumePdfService;
    }

    private static DateTime Utc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static DateTime? Utc(DateTime? value) => value.HasValue ? Utc(value.Value) : null;
    

    public async Task<CandidateIndexResult?> GetIndexAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return null;
        }

        return new CandidateIndexResult
        {
            Profile = new EditProfileViewModel
            {
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                ProfilePicture = user.ProfilePicture
            },
            Resumes = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.UserId == userId)
                .ToListAsync()
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
        {
            return ServiceResult.Failure(new[] { "Пользователь не найден." });
        }

        var errors = new List<string>();

        if (user.UserName != model.Username)
        {
            var existing = await _userManager.FindByNameAsync(model.Username);
            if (existing != null && existing.Id != user.Id)
            {
                errors.Add("Это имя пользователя уже занято.");
            }
        }

        if (user.Email != model.Email)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null && existing.Id != user.Id)
            {
                errors.Add("Этот адрес электронной почты уже используется.");
            }
        }

        if (errors.Any())
        {
            return ServiceResult.Failure(errors);
        }

        user.UserName = model.Username;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.ProfilePicture = string.IsNullOrWhiteSpace(model.ProfilePicture) ? DefaultPicture : model.ProfilePicture;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded
            ? ServiceResult.Success()
            : ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }
    

    public async Task CreateResumeAsync(int userId, CreateResumeViewModel model)
    {
        _context.Resumes.Add(new Resume
        {
            UserId = userId,
            Category = model.Category,
            Title = model.Title,
            ExpectedSalary = model.ExpectedSalary,
            Telegram = model.Telegram,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            Facebook = model.Facebook,
            LinkedIn = model.LinkedIn
        });
        await _context.SaveChangesAsync();
    }

    public async Task<CreateResumeViewModel?> GetResumeForEditAsync(int userId, int resumeId)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            return null;
        }

        return new CreateResumeViewModel
        {
            Category = resume.Category,
            Title = resume.Title,
            ExpectedSalary = resume.ExpectedSalary,
            Telegram = resume.Telegram,
            Email = resume.Email,
            PhoneNumber = resume.PhoneNumber,
            Facebook = resume.Facebook,
            LinkedIn = resume.LinkedIn
        };
    }

    public async Task<bool> UpdateResumeAsync(int userId, int resumeId, CreateResumeViewModel model)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            return false;
        }

        resume.Category = model.Category;
        resume.Title = model.Title;
        resume.ExpectedSalary = model.ExpectedSalary;
        resume.Telegram = model.Telegram;
        resume.Email = model.Email;
        resume.PhoneNumber = model.PhoneNumber;
        resume.Facebook = model.Facebook;
        resume.LinkedIn = model.LinkedIn;
        resume.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<Resume?> GetResumeDetailsAsync(int userId, int resumeId)
    {
        return await _context.Resumes
            .AsNoTracking()
            .Include(r => r.WorkExperiences)
            .Include(r => r.Courses)
            .Include(r => r.Educations)
            .FirstOrDefaultAsync(r => r.Id == resumeId && (r.UserId == userId || r.IsPublished));
    }

    public async Task<ResumePdfFile?> GetResumePdfAsync(int userId, int resumeId)
    {
        var resume = await _context.Resumes
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.WorkExperiences)
            .Include(r => r.Educations)
            .Include(r => r.Courses)
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            return null;
        }

        var safeTitle = new string(resume.Title
            .Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_')
            .ToArray()).Trim('_');
        if (string.IsNullOrWhiteSpace(safeTitle))
        {
            safeTitle = $"Resume_{resume.Id}";
        }

        return new ResumePdfFile(_resumePdfService.Generate(resume), $"{safeTitle}.pdf");
    }

    public async Task<bool> TogglePublishAsync(int userId, int resumeId)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            return false;
        }

        resume.IsPublished = !resume.IsPublished;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefreshAsync(int userId, int resumeId)
    {
        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            return false;
        }

        resume.UpdatedDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> OwnsResumeAsync(int userId, int resumeId)
    {
        return _context.Resumes.AnyAsync(r => r.Id == resumeId && r.UserId == userId);
    }
    

    public async Task<bool> AddWorkExperienceAsync(int userId, int resumeId, CreateWorkExperienceViewModel model)
    {
        if (!await OwnsResumeAsync(userId, resumeId))
        {
            return false;
        }

        _context.WorkExperiences.Add(new WorkExperience
        {
            ResumeId = resumeId,
            CompanyName = model.CompanyName,
            Position = model.Position,
            Description = model.Description,
            StartDate = Utc(model.StartDate),
            EndDate = Utc(model.EndDate)
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EditData<CreateWorkExperienceViewModel>?> GetWorkExperienceForEditAsync(int userId, int id)
    {
        var item = await _context.WorkExperiences
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id && w.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        return new EditData<CreateWorkExperienceViewModel>(item.ResumeId, new CreateWorkExperienceViewModel
        {
            CompanyName = item.CompanyName,
            Position = item.Position,
            Description = item.Description,
            StartDate = item.StartDate,
            EndDate = item.EndDate
        });
    }

    public async Task<int?> UpdateWorkExperienceAsync(int userId, int id, CreateWorkExperienceViewModel model)
    {
        var item = await _context.WorkExperiences
            .FirstOrDefaultAsync(w => w.Id == id && w.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        item.CompanyName = model.CompanyName;
        item.Position = model.Position;
        item.Description = model.Description;
        item.StartDate = Utc(model.StartDate);
        item.EndDate = Utc(model.EndDate);
        await _context.SaveChangesAsync();
        return item.ResumeId;
    }

    public async Task<int?> DeleteWorkExperienceAsync(int userId, int id)
    {
        var item = await _context.WorkExperiences
            .FirstOrDefaultAsync(w => w.Id == id && w.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        var resumeId = item.ResumeId;
        _context.WorkExperiences.Remove(item);
        await _context.SaveChangesAsync();
        return resumeId;
    }
    

    public async Task<bool> AddCourseAsync(int userId, int resumeId, CreateCourseViewModel model)
    {
        if (!await OwnsResumeAsync(userId, resumeId))
        {
            return false;
        }

        _context.Courses.Add(new Course
        {
            ResumeId = resumeId,
            Name = model.Name,
            Organization = model.Organization,
            Description = model.Description,
            StartDate = Utc(model.StartDate),
            EndDate = Utc(model.EndDate)
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EditData<CreateCourseViewModel>?> GetCourseForEditAsync(int userId, int id)
    {
        var item = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        return new EditData<CreateCourseViewModel>(item.ResumeId, new CreateCourseViewModel
        {
            Name = item.Name,
            Organization = item.Organization,
            Description = item.Description,
            StartDate = item.StartDate,
            EndDate = item.EndDate
        });
    }

    public async Task<int?> UpdateCourseAsync(int userId, int id, CreateCourseViewModel model)
    {
        var item = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && c.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        item.Name = model.Name;
        item.Organization = model.Organization;
        item.Description = model.Description;
        item.StartDate = Utc(model.StartDate);
        item.EndDate = Utc(model.EndDate);
        await _context.SaveChangesAsync();
        return item.ResumeId;
    }

    public async Task<int?> DeleteCourseAsync(int userId, int id)
    {
        var item = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == id && c.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        var resumeId = item.ResumeId;
        _context.Courses.Remove(item);
        await _context.SaveChangesAsync();
        return resumeId;
    }
    

    public async Task<bool> AddEducationAsync(int userId, int resumeId, CreateEducationViewModel model)
    {
        if (!await OwnsResumeAsync(userId, resumeId))
        {
            return false;
        }

        _context.Educations.Add(new Education
        {
            ResumeId = resumeId,
            Institution = model.Institution,
            Specialty = model.Specialty,
            Description = model.Description,
            StartDate = Utc(model.StartDate),
            EndDate = Utc(model.EndDate)
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<EditData<CreateEducationViewModel>?> GetEducationForEditAsync(int userId, int id)
    {
        var item = await _context.Educations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && e.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        return new EditData<CreateEducationViewModel>(item.ResumeId, new CreateEducationViewModel
        {
            Institution = item.Institution,
            Specialty = item.Specialty,
            Description = item.Description,
            StartDate = item.StartDate,
            EndDate = item.EndDate
        });
    }

    public async Task<int?> UpdateEducationAsync(int userId, int id, CreateEducationViewModel model)
    {
        var item = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == id && e.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        item.Institution = model.Institution;
        item.Specialty = model.Specialty;
        item.Description = model.Description;
        item.StartDate = Utc(model.StartDate);
        item.EndDate = Utc(model.EndDate);
        await _context.SaveChangesAsync();
        return item.ResumeId;
    }

    public async Task<int?> DeleteEducationAsync(int userId, int id)
    {
        var item = await _context.Educations
            .FirstOrDefaultAsync(e => e.Id == id && e.Resume.UserId == userId);
        if (item == null)
        {
            return null;
        }

        var resumeId = item.ResumeId;
        _context.Educations.Remove(item);
        await _context.SaveChangesAsync();
        return resumeId;
    }
}