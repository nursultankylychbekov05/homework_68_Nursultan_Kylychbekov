using GroupHomework66.Enums;
using GroupHomework66.Models;

namespace GroupHomework66.ViewModels;

public class ResumesCatalogResult
{
    public List<Resume> Resumes { get; set; } = new();
    public List<Vacancy> EmployerVacancies { get; set; } = new();
    public VacancyCategory? SelectedCategory { get; set; }
}