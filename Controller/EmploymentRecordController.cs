using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.EmploymentRecord;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.EmploymentRecords;
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
    public class EmploymentRecordController : Controller
    {
        private readonly IEmploymentRecordServices _employmentRecordServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EmploymentRecordController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="employmentRecordServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        public EmploymentRecordController(IEmploymentRecordServices employmentRecordServices,
            ApplicationDbContext applicationDbContext,
            ILogger<EmploymentRecordController> logger,
            IEmployeeServices employeeServices)
        {
            _db = applicationDbContext;
            _employeeServices = employeeServices;
            _employmentRecordServices = employmentRecordServices;
            _logger = logger;
        }
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<EmploymentRecordListViewModel> employmentList = await GetEmploymentList(search, Employee);
            employmentList = SortEmploymentRecords(sort, employmentList);
            var model = await LoadIndexViewModel(page, employmentList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employmentQuery = await _employmentRecordServices.GetEmploymentRecordById(id);
            if (employmentQuery == null)
            {
                return NotFound();
            }
            await _employmentRecordServices.DeleteEmployeementRecordAsync(employmentQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted employment record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        private async Task<PagedList<IEnumerable<EmploymentRecordListViewModel>, EmploymentRecordListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EmploymentRecordListViewModel> employmentList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = employmentList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        /// <summary>
        /// Loads all employees from the database
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
        }
        /// <summary>
        /// Sorts the list of all employment records.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="employmentList"></param>
        /// <returns></returns>
        private IEnumerable<EmploymentRecordListViewModel> SortEmploymentRecords(string sort, IEnumerable<EmploymentRecordListViewModel> employmentList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_employmentRecord" : "employee";
            switch (sort)
            {
                case "employee":
                    employmentList = employmentList.OrderByDescending(employment => employment.EmployeeID);
                    break;

                case "employee_employmentRecord":
                    employmentList = employmentList.OrderBy(employment => employment.EmployeeID);
                    break;

                default:
                    employmentList = employmentList.OrderBy(employment => employment.EmployeeID);
                    break;
            }
            return employmentList;
        }
        /// <summary>
        /// Returns a list of all employment records.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="employmentList"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EmploymentRecordListViewModel>> GetEmploymentList(string search, string employee)
        {
            var employmentQuery = await _employmentRecordServices.ListEmploymentRecordsAsync();
            employmentQuery = FilterEmployment(search, employee, employmentQuery);

            return employmentQuery.Select(EmploymentQuery => new EmploymentRecordListViewModel
            {
                Id = EmploymentQuery.Id,
                CompanyName = EmploymentQuery.Companyname,
                Employee = EmploymentQuery.Employee.FullName,
                EmployeeID = EmploymentQuery.EmployeeId ?? Guid.Empty,
                PositionHeld = EmploymentQuery.PositionHeld,
                Duration = EmploymentQuery.Duration
            });
        }
        /// <summary>
        /// Returns a filtered list of all employment records.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="employmentList"></param>
        /// <returns></returns>
        private IEnumerable<EmployeementRecord> FilterEmployment(string search, string employee, IEnumerable<EmployeementRecord> employmentQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                employmentQuery = employmentQuery.Where(employment => employment.Companyname.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                employmentQuery = employmentQuery.Where(employment => employment.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }
            return employmentQuery;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string search, string Employee)
        {
            return RedirectToAction("index", new { search, Employee });
        }
        /// <summary>
        /// Add Constructor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        /// <summary>
        /// Adds a new entry of employment records.
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewEmploymentRecordViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                        await _employmentRecordServices.AddEmploymentRecordAsync(new EmployeementRecord
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Companyname = formData.CompanyName,
                            PositionHeld = formData.PositionHeld,
                            Duration = formData.Duration,
                            EmployeeId = formData.EmployeeID,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Employment record added successfully";
                        _logger.LogInformation($"Success: successfully added employee employment details {formData.EmployeeID} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employment record of employee {formData.EmployeeID}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("EmploymentRecord", $"Failed to record employee employment record {formData.EmployeeID}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// The Update Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var employmentRecordQuery = await _employmentRecordServices.GetEmploymentRecordById(id);
            if (employmentRecordQuery == null)
            {
                return NotFound();
            }
            var model = new EmploymentRecordDetailViewModel
            {
                Id = employmentRecordQuery.Id,
                EmployeeID = employmentRecordQuery.EmployeeId ?? Guid.Empty,
                CompanyName = employmentRecordQuery.Companyname,
                PositionHeld = employmentRecordQuery.PositionHeld,
                Duration = employmentRecordQuery.Duration
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        /// <summary>
        /// Updates existing employment records
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(EmploymentRecordDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employmentRecordServices.UpdateEmploymentRecordAsync(new EmployeementRecord
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeID,
                        Companyname = formData.CompanyName,
                        PositionHeld = formData.PositionHeld,
                        Duration = formData.Duration,
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee employment record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee employment details {formData.EmployeeID}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Employment", $"Failed to update employee employment record. {formData.EmployeeID} Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// Catches application errors.
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}