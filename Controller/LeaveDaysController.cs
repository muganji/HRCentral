using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRCentral.Core.Models;
using HRCentral.Services.Leavedays;
using HRCentral.Web.Models;
using HRCentral.Web.Models.LeaveDays;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using HRCentral.Core.Data;
using System.Diagnostics;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
    public class LeaveDaysController : Controller
    {
        private readonly ILeaveDayServices _leaveDayServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<LanguageController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="leaveDayServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        public LeaveDaysController(ILeaveDayServices leaveDayServices, ApplicationDbContext applicationDbContext, ILogger<LanguageController> logger)
        {
            _db = applicationDbContext;
            _leaveDayServices = leaveDayServices;
            _logger = logger;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var leavedays = (await _leaveDayServices.ListLeaveDaysAsync())
                .Select(leaveday => new LeaveDayListViewModel
                {
                    Days = leaveday.Days,
                    Id = leaveday.Id,
                    Title = leaveday.Title,
                    DateAdded = leaveday.DateTimeAdded == null ? string.Empty : DateTime.Parse(leaveday.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = leaveday.DateTimeModified == null ? string.Empty : DateTime.Parse(leaveday.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = leaveday.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(leavedays);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var leaveQuery = await _leaveDayServices.GetLeaveDaysById(id);

            if (leaveQuery == null)
            {
                return NotFound();
            }

            var model = new LeaveDayDetailViewModel
            {
                Days = leaveQuery.Days ?? 0,
                Id = leaveQuery.Id,
                Title = leaveQuery.Title
            };

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var leaveQuery = await _leaveDayServices.GetLeaveDaysById(id);
            if (leaveQuery == null)
            {
                return NotFound();
            }
            await _leaveDayServices.DeleteLeaveDaysAsync(leaveQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted leave days record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(LeaveDayDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _leaveDayServices.UpdateLeaveDaysAsync(new LeaveDays
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Days = formData.Days,
                        Id = formData.Id,
                        Title = formData.Title,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Title} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Bank", $"Failed to update record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Title}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
            }

            return View(formData);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Add()
        {
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewLeaveDayViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.LeaveDays where c.Title == formData.Title || c.Days == formData.Days select c;

                    try
                    {
                        q.ToList()[0].Title.ToString();
                        q.ToList()[0].Days.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("LeaveDays", $"Can not register duplicate record. {formData.Title} or {formData.Days} is already registered");
                    }
                    else
                    {
                        await _leaveDayServices.AddLeaveDaysAsync(new LeaveDays
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Days = formData.Days,
                            Title = formData.Title,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "LeaveDays Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Title} and {formData.Days} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("LeaveDays", $"Failed to register record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Title} Bank. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}