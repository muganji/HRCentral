using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Repatriations;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Repatriations;
using HRCentral.Services.RelationshipTypes;
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
    public class EmployeeRepatriationController : Controller
    {
        private readonly IRepatriationServices _repatriationServices;
        private readonly ApplicationDbContext _db;
        private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly ILogger<EmployeeRepatriationController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="repatriationServices"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        public EmployeeRepatriationController(
            ApplicationDbContext applicationDbContext,
            IRepatriationServices repatriationServices,
            IRelationshipTypeServices relationshipTypeServices,
            ILogger<EmployeeRepatriationController> logger,
            IEmployeeServices employeeServices)
        {
            _db = applicationDbContext;
            _employeeServices = employeeServices;
            _logger = logger;
            _relationshipTypeServices = relationshipTypeServices;
            _repatriationServices = repatriationServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<RepatriationListViewModel> repatriationList = await GetRepatriationList(search, Employee);
            repatriationList = SortRepatriation(sort, repatriationList);
            var model = await LoadIndexViewModel(page, repatriationList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var repatriationQuery = await _repatriationServices.GetRepatriationById(id);
            if (repatriationQuery == null)
            {
                return NotFound();
            }
            await _repatriationServices.DeleteRepatriationAsync(repatriationQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted repatriation record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
      
        public IActionResult Delete()
        {
            return View();
        }
        /// <summary>
        /// Returns all records from the repatriation table
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task<IEnumerable<RepatriationListViewModel>> GetRepatriationList(string search, string employee)
        {
            var repatriationQuery = await _repatriationServices.ListRepatriationAsync();
            repatriationQuery = FilterRepatriation(search, employee, repatriationQuery);

            return repatriationQuery.Select(RepatriationQuery => new RepatriationListViewModel
            {
                Id = RepatriationQuery.Id,
                Employee = RepatriationQuery.Employee.FullName,
                Name = RepatriationQuery.Name,
                RelationshipId = RepatriationQuery.RelationshipId ?? Guid.Empty,
                Relationship = RepatriationQuery.RelationShipType.RelationshipName,
                EmployeeId = RepatriationQuery.EmployeeId ?? Guid.Empty
               
            });
        }
        /// <summary>
        /// Filters queried results from the database.
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="repatriationQuery"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeRepatriation> FilterRepatriation(string search, string employee, IEnumerable<EmployeeRepatriation> repatriationQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                repatriationQuery = repatriationQuery.Where(repatriation => repatriation.Name.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                repatriationQuery = repatriationQuery.Where(repatriation => repatriation.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }
            return repatriationQuery;
        }

        private async Task<PagedList<IEnumerable<RepatriationListViewModel>, RepatriationListViewModel>> LoadIndexViewModel(int? page, IEnumerable<RepatriationListViewModel> repatriationList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = repatriationList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        /// <summary>
        /// Loads all records of existing employees
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
            ViewData["Relationships"] = (await _relationshipTypeServices.ListRelationshipsAsync())
                                          .Select(relationship => new SelectListItem
                                          {
                                              Text = relationship.Title,
                                              Value = relationship.Id.ToString()
                                          });
        }
        /// <summary>
        /// Sorts records queried from database.
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="repatriationList"></param>
        /// <returns></returns>
        private IEnumerable<RepatriationListViewModel> SortRepatriation(string sort, IEnumerable<RepatriationListViewModel> repatriationList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_repatriation" : "employee";
            switch (sort)
            {
                case "employee":
                    repatriationList = repatriationList.OrderByDescending(repatriation => repatriation.EmployeeId);
                    break;

                case "employee_repatriation":
                    repatriationList = repatriationList.OrderBy(repatriation => repatriation.EmployeeId);
                    break;

                default:
                    repatriationList = repatriationList.OrderBy(repatriation => repatriation.EmployeeId);
                    break;
            }
            return repatriationList;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string search, string Employee)
        {
            return RedirectToAction("index", new { search, Employee });
        }
        /// <summary>
        /// The add constructor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        /// <summary>
        /// Adds a new instance of the repatriation class
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewRepatriationViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    bool bIfExist = false;
                    var q = from c in _db.EmployeeRepatriations where c.Name == formData.Name select c;
                    try
                    {
                        q.ToList()[0].Name.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Repatriation", $"Can not register duplicate record. {formData.Name} is already registered");
                    }
                    else
                    {
                        await _repatriationServices.AddRepatriationAsync(new EmployeeRepatriation
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Name = formData.Name,
                            RelationshipId = formData.RelationshipId,
                            EmployeeId = formData.EmployeeId,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Repatriation added successfully";
                        _logger.LogInformation($"Success: successfully added employee repatriation details {formData.Name} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register repatriation with names of {formData.Name}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Repatriation", $"Failed to record employee repatriation with names of {formData.Name}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// The Update Constructor.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var repatriationQuery = await _repatriationServices.GetRepatriationById(id);
            if (repatriationQuery == null)
            {
                return NotFound();
            }
            var model = new RepatriationDetailViewModel
            {
                Id = repatriationQuery.Id,
                EmployeeId = repatriationQuery.EmployeeId ?? Guid.Empty,
                Name = repatriationQuery.Name,
                RelationshipId = repatriationQuery.RelationshipId
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        /// <summary>
        /// Updates existing repatriation entry records.
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(RepatriationDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _repatriationServices.UpdateRepatriationAsync(new EmployeeRepatriation
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeId,
                        Name = formData.Name,
                        RelationshipId = formData.RelationshipId,
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee repatriation record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee repatriation details {formData.Name}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Repatriation", $"Failed to update employee repatriation record. {formData.Name} Contact IT ServiceDesk for support thank you.");
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