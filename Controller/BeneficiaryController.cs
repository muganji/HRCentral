using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Beneficiaries;
using HRCentral.Services.Employees;
using HRCentral.Services.RelationshipTypes;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Beneficiary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class BeneficiaryController : Controller
    {
        private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly IBeneficiaryServices _beneficiaryServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<BeneficiaryController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="relationshipTypeServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        /// <param name="beneficiaryServices"></param>
        public BeneficiaryController(
            IRelationshipTypeServices relationshipTypeServices,
            ApplicationDbContext applicationDbContext,
            ILogger<BeneficiaryController> logger,
            IEmployeeServices employeeServices,
            IBeneficiaryServices beneficiaryServices)
        {
            _db = applicationDbContext;
            _relationshipTypeServices = relationshipTypeServices;
            _logger = logger;
            _employeeServices = employeeServices;
            _beneficiaryServices = beneficiaryServices;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="search"></param>
        /// <param name="Employee"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<BeneficiaryListViewModel> beneficiaryList = await GetBeneficiaryList(search, Employee);
            beneficiaryList = SortBeneficiary(sort, beneficiaryList);
            var model = await LoadIndexViewModel(page, beneficiaryList);
            ViewData["page"] = page;
            return View(model);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="beneficiaryList"></param>
        /// <returns></returns>
        private async Task<PagedList<IEnumerable<BeneficiaryListViewModel>, BeneficiaryListViewModel>> LoadIndexViewModel(int? page, IEnumerable<BeneficiaryListViewModel> beneficiaryList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = beneficiaryList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var beneficiaryQuery = await _beneficiaryServices.GetBeneficiaryDetailById(id);
            if (beneficiaryQuery == null)
            {
                return NotFound();
            }
            await _beneficiaryServices.DeleteBeneficiaryAsync(beneficiaryQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted beneficiary record by user={@User.Identity.Name.Substring(4)}");
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
        /// <param name="sort"></param>
        /// <param name="beneficiaryList"></param>
        /// <returns></returns>
        private IEnumerable<BeneficiaryListViewModel> SortBeneficiary(string sort, IEnumerable<BeneficiaryListViewModel> beneficiaryList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_beneficiary" : "employee";
            switch (sort)
            {
                case "employee":
                    beneficiaryList = beneficiaryList.OrderByDescending(beneficiary => beneficiary.EmployeeId);
                    break;

                case "employee_beneficiary":
                    beneficiaryList = beneficiaryList.OrderBy(beneficiary => beneficiary.EmployeeId);
                    break;

                default:
                    beneficiaryList = beneficiaryList.OrderBy(beneficiary => beneficiary.EmployeeId);
                    break;
            }
            return beneficiaryList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task<IEnumerable<BeneficiaryListViewModel>> GetBeneficiaryList(string search, string employee)
        {
            var beneficiaryQuery = await _beneficiaryServices.ListBeneficiaryDetailsAsync();
            beneficiaryQuery = FilterBeneficiary(search, employee, beneficiaryQuery);

            return beneficiaryQuery.Select(beneficiariesQuery => new BeneficiaryListViewModel
            {
                Id = beneficiariesQuery.Id,
                BeneficiaryName = beneficiariesQuery.BeneficiaryName,
                Employee = beneficiariesQuery.Employee.FullName,
              BeneficiaryContact = beneficiariesQuery.BeneficiaryContact,
                EmployeeId = beneficiariesQuery.EmployeeID ?? Guid.Empty,
               RelationShip = beneficiariesQuery.RelationShipType.RelationshipName
               
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="beneficiaryQuery"></param>
        /// <returns></returns>
        private IEnumerable<BeneficiaryDetails> FilterBeneficiary(string search, string employee, IEnumerable<BeneficiaryDetails> beneficiaryQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                beneficiaryQuery = beneficiaryQuery.Where(beneficiary => beneficiary.BeneficiaryName.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                beneficiaryQuery = beneficiaryQuery.Where(beneficiary => beneficiary.EmployeeID == EmployeeId);
                ViewData["employee"] = employee;
            }
            return beneficiaryQuery;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task LoadSelectListsAsync()
        {
            ViewData["Employees"] = (await _employeeServices.ListEmployeesAsync())
                .Where(emp => emp.IsActive)
                                            .Select(employees => new SelectListItem
                                            {
                                                Text = employees.FirstName + ',' + ' ' + employees.LastName,
                                                Value = employees.Id.ToString()
                                            });
            ViewData["RelationShips"] = (await _relationshipTypeServices.ListRelationshipsAsync())
                                            .Select(relationship => new SelectListItem
                                            {
                                                Text = relationship.Title,
                                                Value = relationship.Id.ToString()
                                            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="Employee"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string search, string Employee)
        {
            return RedirectToAction("index", new { search, Employee });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
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
        public async Task<IActionResult> Add(NewBeneficiaryViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
           
                    bool bIfExist = false;
                    var q = from c in _db.BeneficiaryDetails where c.BeneficiaryName == formData.BeneficiaryName || c.BeneficiaryContact == formData.BeneficiaryContact select c;
                    try
                    {
                        q.ToList()[0].BeneficiaryContact.ToString();
                        q.ToList()[0].BeneficiaryName.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Beneficiary", $"Can not register duplicate record. {formData.BeneficiaryName} or {formData.BeneficiaryContact} is already registered");
                    }

                    else
                    {
                        await _beneficiaryServices.AddBeneficiaryDetailsAsync(new BeneficiaryDetails
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            BeneficiaryName = formData.BeneficiaryName,
                            BeneficiaryContact = formData.BeneficiaryContact,
                            
                            RelationshipId = formData.RelationshipId,
                            EmployeeID = formData.EmployeeID,
                            UserAccount = User.Identity.Name
                            

                        });
                        TempData["Message"] = "Beneficiary added successfully";
                        _logger.LogInformation($"Success: successfully added employee beneficiary details {formData.BeneficiaryName} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                  $"FAIL: failed to register beneficiary with names of {formData.BeneficiaryName}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Beneficiary", $"Failed to record employee beneficiary with names of {formData.BeneficiaryName}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var beneficiaryQuery = await _beneficiaryServices.GetBeneficiaryDetailById(id);
            if (beneficiaryQuery == null)
            {
                return NotFound();
            }

            var model = new BeneficiaryDetailViewModel
            {
                Id = beneficiaryQuery.Id,
                EmployeeID = beneficiaryQuery.EmployeeID ?? Guid.Empty,
                BeneficiaryName = beneficiaryQuery.BeneficiaryName,
                BeneficiaryContact = beneficiaryQuery.BeneficiaryContact,
                RelationshipId = beneficiaryQuery.RelationshipId ?? Guid.Empty
    
            };
            await LoadSelectListsAsync();
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
        public async Task<IActionResult> Details(BeneficiaryDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _beneficiaryServices.UpdateBeneficiaryDetailAsync(new BeneficiaryDetails
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeID = formData.EmployeeID,
                        BeneficiaryName = formData.BeneficiaryName,
                        BeneficiaryContact = formData.BeneficiaryContact,
                        RelationshipId = formData.RelationshipId,
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee beneficiary record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee beneficiary details {formData.BeneficiaryName}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Beneficiary", $"Failed to update employee beneficiary record. {formData.BeneficiaryName} Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// Catches Errors associated with this controller
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}