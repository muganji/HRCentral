using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Contracts;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Contracts;
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
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IContractsServices _contractServices;
        private readonly ILogger<ContractsController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="contractsServices"></param>
        /// <param name="logger"></param>
        public ContractsController(
            ApplicationDbContext applicationDbContext,
            IContractsServices contractsServices,
            ILogger<ContractsController> logger)
        {
            _db = applicationDbContext;
            _contractServices = contractsServices;
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

            var contracts = (await _contractServices.ListContractsAsync())
                .Select(contract => new ContractsListViewModel
                {
                    Title = contract.Title,
                    Months = contract.Month,
                    Id = contract.Id,
                    DateAdded = contract.DateTimeAdded == null ? string.Empty : DateTime.Parse(contract.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = contract.DateTimeModified == null ? string.Empty : DateTime.Parse(contract.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = contract.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(contracts);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var contractQuery = await _contractServices.GetContractsById(id);
            if (contractQuery == null)
            {
                return NotFound();
            }
            await _contractServices.DeleteContractsAsync(contractQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted contracts record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        /// <summary>
        /// Selects contract details by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Details(Guid id)
        {
            var contractQuery = await _contractServices.GetContractsById(id);

            if (contractQuery == null)
            {
                return NotFound();
            }

            var model = new ContractsDetailViewModel
            {
                Title = contractQuery.Title,
                Months = contractQuery.Month,
                Id = contractQuery.Id,
            };

            return View(model);
        }
        /// <summary>
        /// Updates existing contract category data
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ContractsDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _contractServices.UpdateContractAsync(new Contract
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Title = formData.Title,
                        Month = formData.Months,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Title} contract category record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Contract", $"Failed to update record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Title} Contract Category. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
            }

            return View(formData);
        }
        /// <summary>
        /// The Add Constructor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public IActionResult Add()
        {
            return View();
        }
        /// <summary>
        /// Adds a new instance of contract category details
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewContractsViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Contracts where c.Title == formData.Title || c.Month == formData.Months select c;
                    try
                    {
                        q.ToList()[0].Title.ToString();
                        q.ToList()[0].Month.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Contract", $"Can not register duplicate record. {formData.Title} or {formData.Months} is already registered");
                    }
                    else
                    {
                        await _contractServices.AddContractAsync(new Contract
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Title = formData.Title,
                            Month = formData.Months,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "Contract Category Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Title} contract category record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Contract", $"Failed to register record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Title} Contract Category. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}