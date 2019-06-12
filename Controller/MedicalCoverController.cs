using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.MedicalCovers;
using HRCentral.Web.Models;
using HRCentral.Web.Models.MedicalCovers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
    public class MedicalCoverController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IMedicalCoverServices _medicalCoverServices;
        private readonly ILogger<MedicalCoverController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="medicalServices"></param>
        /// <param name="logger"></param>
        /// <param name="applicationDbContext"></param>
        public MedicalCoverController(
             IMedicalCoverServices medicalServices,
             ILogger<MedicalCoverController> logger,
             ApplicationDbContext applicationDbContext)
        {
            _medicalCoverServices = medicalServices;
            _db = applicationDbContext;
            _logger = logger;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var medicals = (await _medicalCoverServices.ListMedicalCoversAsync())
                .Select(medical => new MedicalCoverListViewModel
                {
                    Title = medical.Title,
                    Id = medical.Id,
                    DateAdded = medical.DateTimeAdded == null ? string.Empty : DateTime.Parse(medical.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = medical.DateTimeModified == null ? string.Empty : DateTime.Parse(medical.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = medical.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(medicals);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var medicalcoverQuery = await _medicalCoverServices.GetMedicalCoverById(id);
            if (medicalcoverQuery == null)
            {
                return NotFound();
            }
            await _medicalCoverServices.DeleteMedicalAsync(medicalcoverQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted medicalcover record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var medicalQuery = await _medicalCoverServices.GetMedicalCoverById(id);

            if (medicalQuery == null)
            {
                return NotFound();
            }

            var model = new MedicalCoverDetailViewModel
            {
                Title = medicalQuery.Title,
                Id = medicalQuery.Id,
            };

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(MedicalCoverDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _medicalCoverServices.UpdateMedicalCoverAsync(new MedicalCover
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Title = formData.Title,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Title} Medical record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Medical", $"Failed to update record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Title} MedicalCover. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
            }

            return View(formData);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewMedicalCoverViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.MedicalCovers where c.Title == formData.Title select c;
                    try
                    {
                        q.ToList()[0].Title.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Medical", $"Can not register duplicate record. {formData.Title} MedicalCover is already registered");
                    }
                    else
                    {
                        await _medicalCoverServices.AddMedicalCoverAsync(new MedicalCover
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Title = formData.Title,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "MedicalCover Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Title} medicalcover record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("MedicalCover", $"Failed to register record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Title} MedicalCover. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}