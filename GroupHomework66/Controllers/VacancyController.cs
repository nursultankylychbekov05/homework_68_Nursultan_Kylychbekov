using System.Security.Claims;
using GroupHomework66.Enums;
using GroupHomework66.Services;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GroupHomework66.Controllers;

public class VacancyController : Controller
{
    private readonly IVacancyService _vacancyService;

    public VacancyController(IVacancyService vacancyService)
    {
        _vacancyService = vacancyService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize(Roles = "Company")]
    public IActionResult MyVacancies()
    {
        return RedirectToAction("Index", "Company");
    }

    [HttpGet]
    [Authorize(Roles = "Company")]
    public IActionResult Create()
    {
        return View(new CreateVacancyViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Create(CreateVacancyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await _vacancyService.CreateAsync(CurrentUserId, model);
        return RedirectToAction(nameof(MyVacancies));
    }

    [Authorize(Roles = "Applicant,Company")]
    public async Task<IActionResult> Details(int id)
    {
        if (User.IsInRole("Company"))
        {
            var own = await _vacancyService.GetEmployerVacancyAsync(CurrentUserId, id);
            return own == null ? NotFound() : View(own);
        }

        var details = await _vacancyService.GetApplicantVacancyDetailsAsync(CurrentUserId, id);
        if (details == null)
        {
            return NotFound();
        }

        ViewBag.UserResumes = details.Resumes
            .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Title })
            .ToList();
        return View(details.Vacancy);
    }

    [HttpGet]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _vacancyService.GetForEditAsync(CurrentUserId, id);
        if (model == null)
        {
            return NotFound();
        }

        ViewBag.VacancyId = id;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Edit(int id, CreateVacancyViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.VacancyId = id;
            return View(model);
        }

        return await _vacancyService.UpdateAsync(CurrentUserId, id, model)
            ? RedirectToAction(nameof(MyVacancies))
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Refresh(int id)
    {
        return await _vacancyService.RefreshAsync(CurrentUserId, id)
            ? RedirectToAction(nameof(MyVacancies))
            : NotFound();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> TogglePublish(int id)
    {
        return await _vacancyService.TogglePublishAsync(CurrentUserId, id)
            ? RedirectToAction(nameof(MyVacancies))
            : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> List(string? search, VacancyCategory? category, string? sortBySalary, int page = 1)
    {
        int? applicantId = User.Identity?.IsAuthenticated == true && User.IsInRole("Applicant")
            ? CurrentUserId
            : null;

        var result = await _vacancyService.GetPublishedListAsync(search, category, sortBySalary, page, applicantId);

        if (applicantId.HasValue)
        {
            ViewBag.UserResumes = new SelectList(result.Resumes, "Id", "Title");
        }

        ViewBag.CurrentSearch = search;
        ViewBag.CurrentCategory = category;
        ViewBag.CurrentSort = sortBySalary;
        ViewBag.CurrentPage = result.Page;
        ViewBag.TotalPages = result.TotalPages;

        return View(result.Vacancies);
    }
}