using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Banks;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Banks;
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
    public class BankController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBankServices _bankServices;
        private readonly ILogger<BankController> _logger;

        /// <summary>
        ///
        /// </summary>
        /// <param name="bankServices"></param>
        public BankController(
            IBankServices bankServices,
            ILogger<BankController> logger,
            ApplicationDbContext applicationDbContext)
        {
            _bankServices = bankServices;
            _db = applicationDbContext;
            _logger = logger;
        }
        
        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var banks = (await _bankServices.ListBankAsync())
                .Select(bank => new BankListViewModel
                {
                    Name = bank.Name,
                    Id = bank.Id,
                    DateAdded = bank.DateTimeAdded == null ? string.Empty : DateTime.Parse(bank.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = bank.DateTimeModified == null ? string.Empty : DateTime.Parse(bank.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = bank.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(banks);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var bankQuery = await _bankServices.GetBankById(id);

            if (bankQuery == null)
            {
                return NotFound();
            }

            var model = new BankDetailViewModel
            {
                Name = bankQuery.Name,
                Id = bankQuery.Id,
            };

            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var bankQuery = await _bankServices.GetBankById(id);
            if(bankQuery == null)
            {
                return NotFound();
            }
            await _bankServices.DeleteBankAsync(bankQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted bank record by user={@User.Identity.Name.Substring(4)}");
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
        public async Task<IActionResult> Details(BankDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _bankServices.UpdateBankAsync(new Bank
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Name = formData.Name,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Name} bank record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Bank", $"Failed to update record. {formData.Name} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Name} Bank. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
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
        public async Task<IActionResult> Add(NewBankViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Banks where c.Name == formData.Name select c;
                    try
                    {
                        q.ToList()[0].Name.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Bank", $"Can not register duplicate record. {formData.Name} Bank is already registered");
                    }
                    else
                    {
                        await _bankServices.AddBankAsync(new Bank
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Name = formData.Name,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "Bank Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Name} bank record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Bank", $"Failed to register record. {formData.Name} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Name} Bank. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}