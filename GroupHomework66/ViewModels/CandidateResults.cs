using GroupHomework66.Models;

namespace GroupHomework66.ViewModels;

public record EditData<T>(int ResumeId, T Model);

public record ResumePdfFile(byte[] Content, string FileName);

public class CandidateIndexResult
{
    public List<Resume> Resumes { get; set; } = new();
    public EditProfileViewModel Profile { get; set; } = new();
}