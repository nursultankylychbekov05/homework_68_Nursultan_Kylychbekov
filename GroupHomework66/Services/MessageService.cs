using GroupHomework66.Models;
using GroupHomework66.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GroupHomework66.Services;

public class MessageService : IMessageService
{
    private readonly SiteMobileContext _context;

    public MessageService(SiteMobileContext context)
    {
        _context = context;
    }

    public async Task<ChatAccess> CheckAccessAsync(int currentUserId, int candidateId, int employerId)
    {
        if (currentUserId != candidateId && currentUserId != employerId)
        {
            return ChatAccess.Forbidden;
        }

        var hasResponse = await _context.Responses
            .AnyAsync(r => r.Resume != null && r.Resume.UserId == candidateId &&
                           r.Vacancy != null && r.Vacancy.EmployerId == employerId);

        return hasResponse ? ChatAccess.Allowed : ChatAccess.NoResponse;
    }

    public async Task<MessageListResult> GetMessagesAsync(int currentUserId, int candidateId, int employerId)
    {
        var access = await CheckAccessAsync(currentUserId, candidateId, employerId);
        if (access != ChatAccess.Allowed)
        {
            return new MessageListResult { Access = access };
        }

        var messages = await _context.Messages
            .AsNoTracking()
            .Where(m => (m.SenderUserId == candidateId && m.ReceiverUserId == employerId) ||
                        (m.SenderUserId == employerId && m.ReceiverUserId == candidateId))
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id,
                m.MessageText,
                m.CreatedAt,
                m.User != null ? m.User.UserName : null,
                m.SenderUserId,
                m.ReceiverUserId))
            .ToListAsync();

        return new MessageListResult { Access = ChatAccess.Allowed, Messages = messages };
    }

    public async Task<ChatAccess> SendAsync(int currentUserId, int candidateId, int employerId, string messageText)
    {
        if (currentUserId != candidateId && currentUserId != employerId)
        {
            return ChatAccess.Forbidden;
        }

        var receiverId = currentUserId == candidateId ? employerId : candidateId;
        if (receiverId == currentUserId)
        {
            return ChatAccess.SelfMessage;
        }

        var access = await CheckAccessAsync(currentUserId, candidateId, employerId);
        if (access != ChatAccess.Allowed)
        {
            return access;
        }

        _context.Messages.Add(new Message
        {
            SenderUserId = currentUserId,
            ReceiverUserId = receiverId,
            MessageText = messageText.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return ChatAccess.Allowed;
    }
}