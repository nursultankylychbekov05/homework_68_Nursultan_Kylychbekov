namespace GroupHomework66.ViewModels;

public enum ChatAccess
{
    Allowed,
    Forbidden,   
    NoResponse,  
    SelfMessage  
}

public record MessageDto(int Id, string MessageText, DateTime CreatedAt, string? SenderUserName,
    int SenderUserId, int ReceiverUserId);

public class MessageListResult
{
    public ChatAccess Access { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}