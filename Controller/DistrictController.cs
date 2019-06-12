using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Districts;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Districts;
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
    public class DistrictController : Controller
    {
        private readonly IDistrictServices _districtServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DistrictController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="districtServices"></param>
        public DistrictController(IDistrictServices districtServices, ILogger<DistrictController> logger, ApplicationDbContext applicationDbContext)
        {
            _districtServices = districtServices;
            _logger = logger;
            _db = applicationDbContext;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;
            var model = (await _districtServices.ListDistrictsAsync())
                .Select(district => new DistrictListViewModel
                {
                    Id = district.Id,
                    Name = district.Name,
                    DateAdded = district.DateTimeAdded == null ? string.Empty : DateTime.Parse(district.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = district.DateTimeModified == null ? string.Empty : DateTime.Parse(district.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = district.UserAccount
                }).ToPagedList(pageSize, pageNumber);
                //.OrderBy(district => district.Name);
            return View(model);
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
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var districtQuery = await _districtServices.GetDistrictById(id);
            if (districtQuery == null)
            {
                return NotFound();
            }
            await _districtServices.DeleteDistrictAsync(districtQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted district record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        /// <summary>
        ///
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewDistrictViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Districts where c.Name == formData.Name select c;
                    try
                    {
                        q.ToList()[0].Name.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Districts", $"Can not register duplicate record. {formData.Name} is already registered");
                    }
                    else
                    {
                        await _districtServices.AddDistrictAsync(new District
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Name = formData.Name,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "District successfully added.";
                        _logger.LogInformation($"Success: successfully added {formData.Name} district record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register {formData.Name} district. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("District", $"Failed register {formData.Name} district record. Contact IT ServiceDesk for support thank you.");
            }
            return View(formData);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
      [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Details(Guid id)
        {
            var districtQuery = await _districtServices.GetDistrictById(id);
            if (districtQuery == null)
            {
                return NotFound();
            }
            var model = new DistrictDetailViewModel
            {
                Id = districtQuery.Id,
                Name = districtQuery.Name
            };
            return View(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="formdata"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(DistrictDetailViewModel formdata)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _districtServices.UpdateDistrictAsync(new District
                    {
                        Id = formdata.Id,
                        Name = formdata.Name,
                        DateTimeModified = DateTimeOffset.Now,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated district {formdata.Name} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formdata.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update {formdata.Name} district. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("District", $"Failed to update {formdata.Name} district record. Contact IT ServiceDesk for support thank you.");
            }
            return View(formdata);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}