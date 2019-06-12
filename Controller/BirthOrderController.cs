using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.BirthOrder;
using HRCentral.Web.Models;
using HRCentral.Web.Models.BirthOrder;
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
    public class BirthOrderController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBirthOrderServices _birthOrderServices;
        private readonly ILogger<BirthOrderController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="birthOrderServices"></param>
        /// <param name="logger"></param>
        /// <param name="applicationDbContext"></param>
        public BirthOrderController(IBirthOrderServices birthOrderServices, ILogger<BirthOrderController> logger, ApplicationDbContext applicationDbContext)
        {
            _birthOrderServices = birthOrderServices;
            _db = applicationDbContext;
            _logger = logger;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var births = (await _birthOrderServices.ListBirthOrdersAsync())
                .Select(birth => new BirthOrderListViewModel
                {
                    Name = birth.BirthOrder,
                    Id = birth.Id,
                    DateAdded = birth.DateTimeAdded == null ? string.Empty : DateTime.Parse(birth.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = birth.DateTimeModified == null ? string.Empty : DateTime.Parse(birth.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = birth.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(births);
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var birthorderQuery = await _birthOrderServices.GetBirthOrderById(id);
            if (birthorderQuery == null)
            {
                return NotFound();
            }
            await _birthOrderServices.DeleteBirthOrderAsync(birthorderQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted birth order record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var birthQuery = await _birthOrderServices.GetBirthOrderById(id);

            if (birthQuery == null)
            {
                return NotFound();
            }

            var model = new BirthOrderDetailViewModel
            {
                Name = birthQuery.BirthOrder,
                Id = birthQuery.Id,
            };

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(BirthOrderDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _birthOrderServices.UpdateBirthOrderAsync(new BirthOrders
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        BirthOrder = formData.Name,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Name} birth order record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Birth", $"Failed to update record. {formData.Name} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Name} Birth Order. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
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
        public async Task<IActionResult> Add(NewBirtOrderViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.BirthOrders where c.BirthOrder == formData.Name select c;
                    try
                    {
                        q.ToList()[0].BirthOrder.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Birth", $"Can not register duplicate record. {formData.Name} Birth Order is already registered");
                    }
                    else
                    {
                        await _birthOrderServices.AddBirthOrderAsync(new BirthOrders
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                           BirthOrder = formData.Name,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "Bith Order Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Name} birth order record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("BirthOrder", $"Failed to register record. {formData.Name} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Name} BirthOrder. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}