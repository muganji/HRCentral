using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Qualifications;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Qualifications;
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
    public class QualificationController : Controller
    {
        private readonly IQualificationServices _qualificationServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<QualificationController> _logger;

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="qualificationServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        public QualificationController(IQualificationServices qualificationServices, ApplicationDbContext applicationDbContext, ILogger<QualificationController> logger)
        {
            _db = applicationDbContext;
            _qualificationServices = qualificationServices;
            _logger = logger;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var model = (await _qualificationServices.ListQualificationsAsync())
                .Select(qualification => new QualificationListViewModel
                {
                    Id = qualification.Id,
                    Title = qualification.Title,
                    DateAdded = qualification.DateTimeAdded == null ? string.Empty : DateTime.Parse(qualification.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = qualification.DateTimeModified == null ? string.Empty : DateTime.Parse(qualification.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = qualification.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var qualificationQuery = await _qualificationServices.GetQualificationById(id);
            if (qualificationQuery == null)
            {
                return NotFound();
            }
            await _qualificationServices.DeleteQualificationAsync(qualificationQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted qualification record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
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
        public async Task<IActionResult> Add(NewQualificationViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Qualifications where c.Title == formData.Title select c;
                    try
                    {
                        q.ToList()[0].Title.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Qualification", $"Can not register duplicate record. {formData.Title} is already registered");
                    }
                    else
                    {
                        await _qualificationServices.AddQualificationAsync(new Qualification
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Title = formData.Title,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Qualification successfully added";
                        _logger.LogInformation($"Successfully added qualification record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
               _logger.LogError(
                  error,
                  $"FAIL: failed to register qualification of {formData.Title}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Qualification", $"Failed to register qualification of {formData.Title}. Contact IT ServiceDesk for support thank you.");
            }
            return View(formData);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public IActionResult Add()
        {
            return View();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Details(Guid id)
        {
            var qualificationQuery = await _qualificationServices.GetQualificationById(id);
            if (qualificationQuery == null)
            {
                return NotFound();
            }
            var model = new QualificationDetailViewModel
            {
                Id = qualificationQuery.Id,
                Title = qualificationQuery.Title
            };
            return View(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(QualificationDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _qualificationServices.UpdateQualificationAsync(new Qualification
                    {
                        DateTimeAdded = DateTimeOffset.Now,
                        DateTimeModified = DateTimeOffset.Now,
                        UserAccount = User.Identity.Name,
                        Title = formData.Title,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Successfully updated qualification record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update qualification {formData.Title}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Qualification", $"Failed to update qualification of {formData.Title}. Contact IT ServiceDesk for support thank you.");
            }
            return View(formData);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}