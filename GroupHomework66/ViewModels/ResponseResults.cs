namespace GroupHomework66.ViewModels;

public class ResponseOperationResult
{
    public bool Succeeded { get; set; }
    public bool NotFound { get; set; }
    public string? Error { get; set; }

    public static ResponseOperationResult Ok() => new() { Succeeded = true };
    public static ResponseOperationResult Missing(string error) => new() { NotFound = true, Error = error };
    public static ResponseOperationResult Fail(string error) => new() { Error = error };
}

public record CompanyResponseDto(int Id, DateTime ResponseDate, int VacancyId, string? VacancyTitle, int ResumeId);

public record ApplicantResponseDto(int Id, DateTime ResponseDate, int ResumeId, string? ResumeTitle,
    int VacancyId, string? VacancyTitle);