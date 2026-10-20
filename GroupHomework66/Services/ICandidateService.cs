using GroupHomework66.Models;
using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface ICandidateService
{
    Task<CandidateIndexResult?> GetIndexAsync(int userId);
    Task<ServiceResult> UpdateProfileAsync(int userId, EditProfileViewModel model);
    
    Task CreateResumeAsync(int userId, CreateResumeViewModel model);
    Task<CreateResumeViewModel?> GetResumeForEditAsync(int userId, int resumeId);
    Task<bool> UpdateResumeAsync(int userId, int resumeId, CreateResumeViewModel model);
    Task<Resume?> GetResumeDetailsAsync(int userId, int resumeId);
    Task<ResumePdfFile?> GetResumePdfAsync(int userId, int resumeId);
    Task<bool> TogglePublishAsync(int userId, int resumeId);
    Task<bool> RefreshAsync(int userId, int resumeId);
    Task<bool> OwnsResumeAsync(int userId, int resumeId);
    
    Task<bool> AddWorkExperienceAsync(int userId, int resumeId, CreateWorkExperienceViewModel model);
    Task<EditData<CreateWorkExperienceViewModel>?> GetWorkExperienceForEditAsync(int userId, int id);
    Task<int?> UpdateWorkExperienceAsync(int userId, int id, CreateWorkExperienceViewModel model);
    Task<int?> DeleteWorkExperienceAsync(int userId, int id);
    
    Task<bool> AddCourseAsync(int userId, int resumeId, CreateCourseViewModel model);
    Task<EditData<CreateCourseViewModel>?> GetCourseForEditAsync(int userId, int id);
    Task<int?> UpdateCourseAsync(int userId, int id, CreateCourseViewModel model);
    Task<int?> DeleteCourseAsync(int userId, int id);
    
    Task<bool> AddEducationAsync(int userId, int resumeId, CreateEducationViewModel model);
    Task<EditData<CreateEducationViewModel>?> GetEducationForEditAsync(int userId, int id);
    Task<int?> UpdateEducationAsync(int userId, int id, CreateEducationViewModel model);
    Task<int?> DeleteEducationAsync(int userId, int id);
}