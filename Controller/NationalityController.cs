using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Nationalities;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Nationalities;
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
    //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class NationalityController : Controller
    {
        private readonly INationalityServices _nationalityServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NationalityController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="nationalityServices"></param>
        public NationalityController(INationalityServices nationalityServices, ILogger<NationalityController> logger, ApplicationDbContext applicationDbContext)
        {
            _nationalityServices = nationalityServices;
            _logger = logger;
            _db = applicationDbContext;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var model = (await _nationalityServices.ListNationalitiesAsync())
                .Select(nationality => new NationalityListViewModel
                {
                    Id = nationality.Id,
                    Description = nationality.Description,
                    DateAdded = nationality.DateTimeAdded == null ? string.Empty : DateTime.Parse(nationality.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = nationality.DateTimeModified == null ? string.Empty : DateTime.Parse(nationality.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = nationality.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var nationalityQuery = await _nationalityServices.GetNationalityById(id);
            if (nationalityQuery == null)
            {
                return NotFound();
            }
            await _nationalityServices.DeleteNationalityAsync(nationalityQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted nationality record by user={@User.Identity.Name.Substring(4)}");
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
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Add(NewNationalityViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Nationalities where c.Description == formData.Description select c;
                    try
                    {
                        q.ToList()[0].Description.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Nationality", $"Can not register duplicate record. {formData.Description} is already registered");
                    }
                    else
                    {
                        await _nationalityServices.AddnationalityAsync(new Nationality
                        {
                            Description = formData.Description,
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Nationality successfully added";
                        _logger.LogInformation($"Successfully added nationality {formData.Description} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register Nationality {formData.Description}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Nationality", $"Failed to register Nationality {formData.Description}. Contact IT ServiceDesk for support thank you.");
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
            _logger.LogInformation($"Initializing the Add Ntaionality Action, user={@User.Identity.Name.Substring(4)}");
            return View();
        }

        public async Task<IActionResult> Details(Guid id)
        {
            _logger.LogInformation($"Initializing the Edit Nationality Action, user={@User.Identity.Name.Substring(4)}");
            var nationalityQuery = await _nationalityServices.GetNationalityById(id);
            if (nationalityQuery == null)
            {
                return NotFound();
            }
            var model = new NationalityDetailViewModel
            {
                Id = nationalityQuery.Id,
                Description = nationalityQuery.Description
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
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(NationalityDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _nationalityServices.UpdateNationalityAsync(new Nationality
                    {
                        Id = formData.Id,
                        DateTimeModified = DateTimeOffset.Now,
                        Description = formData.Description,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Successfully updated nationality {formData.Description} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update nationality {formData.Description}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Nationality", $"Failed to update nationality record {formData.Description}. Contact IT ServiceDesk for support thank you.");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}