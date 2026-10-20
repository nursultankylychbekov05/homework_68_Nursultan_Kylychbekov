using System.Security.Claims;
using GroupHomework66.Enums;
using GroupHomework66.Services;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupHomework66.Controllers;

[Authorize(Roles = "Company")]
public class CompanyController : Controller
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index()
    {
        var model = await _companyService.GetProfileAsync(CurrentUserId);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditCompanyProfileViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _companyService.UpdateProfileAsync(CurrentUserId, model);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        var profile = await _companyService.GetProfileAsync(CurrentUserId, model);
        if (profile == null)
        {
            return NotFound();
        }

        ViewBag.OpenModal = true;
        return View("Index", profile);
    }

    public async Task<IActionResult> ResumesCatalog(VacancyCategory? category)
    {
        var result = await _companyService.GetResumesCatalogAsync(CurrentUserId, category);

        ViewBag.EmployerVacancies = result.EmployerVacancies;
        ViewBag.SelectedCategory = result.SelectedCategory;

        return View(result.Resumes);
    }
}