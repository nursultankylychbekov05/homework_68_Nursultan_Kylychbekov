using GroupHomework66.Models;
using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface IResponseService
{
    Task<ResponseOperationResult> CreateAsync(int applicantId, int vacancyId, int resumeId);
    Task<ResponseOperationResult> RemoveAsync(int applicantId, int vacancyId);
    Task<List<CompanyResponseDto>> GetCompanyResponsesAsync(int employerId);
    Task<List<ApplicantResponseDto>> GetApplicantResponsesAsync(int applicantId);
    Task<CompanyResponsesViewModel> GetCompanyResponsesPageAsync(int employerId);
    Task<Response?> GetResponseForEmployerAsync(int employerId, int responseId);
    Task<List<Response>> GetApplicantResponsesWithDetailsAsync(int applicantId);
}