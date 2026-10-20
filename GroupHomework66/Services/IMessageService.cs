using GroupHomework66.ViewModels;

namespace GroupHomework66.Services;

public interface IMessageService
{
    Task<ChatAccess> CheckAccessAsync(int currentUserId, int candidateId, int employerId);
    Task<MessageListResult> GetMessagesAsync(int currentUserId, int candidateId, int employerId);
    Task<ChatAccess> SendAsync(int currentUserId, int candidateId, int employerId, string messageText);
}