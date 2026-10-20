using System.Security.Claims;
using GroupHomework66.Services;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupHomework66.Controllers;

[Authorize]
public class ResponseController : Controller
{
    private readonly IResponseService _responseService;

    public ResponseController(IResponseService responseService)
    {
        _responseService = responseService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private IActionResult ToHttp(ResponseOperationResult result)
    {
        if (result.Succeeded)
        {
            return Ok();
        }

        return result.NotFound ? NotFound(result.Error) : BadRequest(result.Error);
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Create(int vacancyId, int resumeId)
    {
        return ToHttp(await _responseService.CreateAsync(CurrentUserId, vacancyId, resumeId));
    }

    [HttpPost]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Remove(int vacancyId)
    {
        return ToHttp(await _responseService.RemoveAsync(CurrentUserId, vacancyId));
    }
    
    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> GetResponsesForCompanyAjax(int? companyId)
    {
        return Json(await _responseService.GetCompanyResponsesAsync(CurrentUserId));
    }
    
    [HttpPost]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> GetResponsesForApplicantAjax(int? applicantId)
    {
        return Json(await _responseService.GetApplicantResponsesAsync(CurrentUserId));
    }

    [Authorize(Roles = "Company")]
    public async Task<IActionResult> CompanyResponses()
    {
        return View(await _responseService.GetCompanyResponsesPageAsync(CurrentUserId));
    }

    [Authorize(Roles = "Company")]
    public async Task<IActionResult> ViewResume(int id)
    {
        var response = await _responseService.GetResponseForEmployerAsync(CurrentUserId, id);
        return response == null ? NotFound() : View(response);
    }

    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> ApplicantResponses()
    {
        return View(await _responseService.GetApplicantResponsesWithDetailsAsync(CurrentUserId));
    }
}