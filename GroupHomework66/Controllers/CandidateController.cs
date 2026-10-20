using System.Security.Claims;
using GroupHomework66.Services;
using GroupHomework66.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroupHomework66.Controllers;

public class CandidateController : Controller
{
    private readonly ICandidateService _candidateService;

    public CandidateController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    

    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Index()
    {
        var result = await _candidateService.GetIndexAsync(CurrentUserId);
        if (result == null)
        {
            return NotFound();
        }

        ViewData["EditProfile"] = result.Profile;
        return View(result.Resumes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["EditProfileError"] = "Проверьте правильность заполнения полей.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _candidateService.UpdateProfileAsync(CurrentUserId, model);
        if (!result.Succeeded)
        {
            TempData["EditProfileError"] = result.Errors.Any()
                ? string.Join(" ", result.Errors)
                : "Не удалось сохранить изменения.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public IActionResult CreateResume()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> CreateResume(CreateResumeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["EditProfileError"] = "Проверьте правильность заполнения полей.";
            return RedirectToAction(nameof(Index));
        }

        await _candidateService.CreateResumeAsync(CurrentUserId, model);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditResume(int id)
    {
        var model = await _candidateService.GetResumeForEditAsync(CurrentUserId, id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditResume(int id, CreateResumeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return await _candidateService.UpdateResumeAsync(CurrentUserId, id, model)
            ? RedirectToAction(nameof(Index))
            : NotFound();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Details(int id)
    {
        var resume = await _candidateService.GetResumeDetailsAsync(CurrentUserId, id);
        return resume == null ? NotFound() : View(resume);
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> DownloadPdf(int id)
    {
        var file = await _candidateService.GetResumePdfAsync(CurrentUserId, id);
        return file == null ? NotFound() : File(file.Content, "application/pdf", file.FileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Publish(int id, string? returnUrl)
    {
        if (!await _candidateService.TogglePublishAsync(CurrentUserId, id))
        {
            return NotFound();
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> Update(int id)
    {
        return await _candidateService.RefreshAsync(CurrentUserId, id)
            ? RedirectToAction("Index", "Candidate")
            : NotFound();
    }
    

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddWorkExperience(int resumeId)
    {
        if (!await _candidateService.OwnsResumeAsync(CurrentUserId, resumeId))
        {
            return NotFound();
        }

        ViewBag.ResumeId = resumeId;
        return View(new CreateWorkExperienceViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddWorkExperience(int resumeId, CreateWorkExperienceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = resumeId;
            return View(model);
        }

        return await _candidateService.AddWorkExperienceAsync(CurrentUserId, resumeId, model)
            ? RedirectToAction(nameof(Details), new { id = resumeId })
            : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditWorkExperience(int id)
    {
        var data = await _candidateService.GetWorkExperienceForEditAsync(CurrentUserId, id);
        if (data == null)
        {
            return NotFound();
        }

        ViewBag.ResumeId = data.ResumeId;
        return View(data.Model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditWorkExperience(int id, CreateWorkExperienceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var data = await _candidateService.GetWorkExperienceForEditAsync(CurrentUserId, id);
            if (data == null)
            {
                return NotFound();
            }

            ViewBag.ResumeId = data.ResumeId;
            return View(model);
        }

        var resumeId = await _candidateService.UpdateWorkExperienceAsync(CurrentUserId, id, model);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> DeleteWorkExperience(int id)
    {
        var resumeId = await _candidateService.DeleteWorkExperienceAsync(CurrentUserId, id);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }
    

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddCourse(int resumeId)
    {
        if (!await _candidateService.OwnsResumeAsync(CurrentUserId, resumeId))
        {
            return NotFound();
        }

        ViewBag.ResumeId = resumeId;
        return View(new CreateCourseViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddCourse(int resumeId, CreateCourseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = resumeId;
            return View(model);
        }

        return await _candidateService.AddCourseAsync(CurrentUserId, resumeId, model)
            ? RedirectToAction(nameof(Details), new { id = resumeId })
            : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditCourse(int id)
    {
        var data = await _candidateService.GetCourseForEditAsync(CurrentUserId, id);
        if (data == null)
        {
            return NotFound();
        }

        ViewBag.ResumeId = data.ResumeId;
        return View(data.Model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditCourse(int id, CreateCourseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var data = await _candidateService.GetCourseForEditAsync(CurrentUserId, id);
            if (data == null)
            {
                return NotFound();
            }

            ViewBag.ResumeId = data.ResumeId;
            return View(model);
        }

        var resumeId = await _candidateService.UpdateCourseAsync(CurrentUserId, id, model);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var resumeId = await _candidateService.DeleteCourseAsync(CurrentUserId, id);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }
    

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddEducation(int resumeId)
    {
        if (!await _candidateService.OwnsResumeAsync(CurrentUserId, resumeId))
        {
            return NotFound();
        }

        ViewBag.ResumeId = resumeId;
        return View(new CreateEducationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> AddEducation(int resumeId, CreateEducationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ResumeId = resumeId;
            return View(model);
        }

        return await _candidateService.AddEducationAsync(CurrentUserId, resumeId, model)
            ? RedirectToAction(nameof(Details), new { id = resumeId })
            : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditEducation(int id)
    {
        var data = await _candidateService.GetEducationForEditAsync(CurrentUserId, id);
        if (data == null)
        {
            return NotFound();
        }

        ViewBag.ResumeId = data.ResumeId;
        return View(data.Model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> EditEducation(int id, CreateEducationViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var data = await _candidateService.GetEducationForEditAsync(CurrentUserId, id);
            if (data == null)
            {
                return NotFound();
            }

            ViewBag.ResumeId = data.ResumeId;
            return View(model);
        }

        var resumeId = await _candidateService.UpdateEducationAsync(CurrentUserId, id, model);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Applicant")]
    public async Task<IActionResult> DeleteEducation(int id)
    {
        var resumeId = await _candidateService.DeleteEducationAsync(CurrentUserId, id);
        return resumeId == null ? NotFound() : RedirectToAction(nameof(Details), new { id = resumeId });
    }
}