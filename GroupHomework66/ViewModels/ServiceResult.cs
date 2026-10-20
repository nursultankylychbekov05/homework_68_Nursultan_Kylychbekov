namespace GroupHomework66.ViewModels;

public class ServiceResult
{
    public bool Succeeded { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ServiceResult Success() => new() { Succeeded = true };

    public static ServiceResult Failure(IEnumerable<string> errors) =>
        new() { Succeeded = false, Errors = errors.ToList() };
}