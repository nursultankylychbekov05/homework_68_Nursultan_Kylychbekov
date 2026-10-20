using GroupHomework66.Models;

namespace GroupHomework66.ViewModels;

public record ResumeOption(int Id, string Title);

public class VacancyListResult
{
    public List<Vacancy> Vacancies { get; set; } = new();
    public List<ResumeOption> Resumes { get; set; } = new();
    public int Page { get; set; }
    public int TotalPages { get; set; }
}

public class ApplicantVacancyDetails
{
    public Vacancy Vacancy { get; set; } = null!;
    public List<ResumeOption> Resumes { get; set; } = new();
}