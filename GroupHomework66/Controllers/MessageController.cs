using System.Security.Claims;
using GroupHomework66.Services;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupHomework66.Controllers;

[Authorize]
public class MessageController : Controller
{
    private readonly IMessageService _messageService;

    public MessageController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Index(int candidateId, int employerId)
    {
        var access = await _messageService.CheckAccessAsync(CurrentUserId, candidateId, employerId);
        if (access == ChatAccess.Forbidden)
        {
            return Forbid();
        }

        if (access == ChatAccess.NoResponse)
        {
            return BadRequest("Доступ к чату ограничен: отсутствует отклик на вакансию.");
        }

        ViewBag.CandidateId = candidateId;
        ViewBag.EmployerId = employerId;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetMessageAjax(int? candidateId, int? employerId)
    {
        if (!candidateId.HasValue || !employerId.HasValue)
        {
            return BadRequest("Не переданы необходимые идентификаторы.");
        }

        var result = await _messageService.GetMessagesAsync(CurrentUserId, candidateId.Value, employerId.Value);

        return result.Access switch
        {
            ChatAccess.Forbidden => Forbid(),
            ChatAccess.NoResponse => BadRequest("Доступ к чату ограничен: отсутствует отклик на вакансию."),
            _ => Json(result.Messages)
        };
    }

    [HttpPost]
    public async Task<IActionResult> SendMessageAjax(int candidateId, int employerId, string messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
        {
            return BadRequest("Сообщение не может быть пустым.");
        }

        var access = await _messageService.SendAsync(CurrentUserId, candidateId, employerId, messageText);

        return access switch
        {
            ChatAccess.Forbidden => Forbid(),
            ChatAccess.SelfMessage => BadRequest("Нельзя отправить сообщение самому себе."),
            ChatAccess.NoResponse => BadRequest("Отправка сообщений возможна только при наличии отклика на вакансию."),
            _ => Ok()
        };
    }
}