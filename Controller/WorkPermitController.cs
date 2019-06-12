using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Employees;
using HRCentral.Services.WorkPermits;
using HRCentral.Web.Models;
using HRCentral.Web.Models.WorkPermits;
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
    public class WorkPermitController : Controller
    {
        private readonly IEmployeeServices _employeeServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<WorkPermitController> _logger;
        private readonly IWorkPermitServices _workPermitServices;

        /// <summary>
        ///
        /// </summary>
        /// <param name="employeeServices"></param>
        /// <param name="workPermitServices"></param>
        public WorkPermitController(IEmployeeServices employeeServices, IWorkPermitServices workPermitServices, ILogger<WorkPermitController> logger, ApplicationDbContext applicationDbContext)
        {
            _employeeServices = employeeServices;
            _workPermitServices = workPermitServices;
            _logger = logger;
            _db = applicationDbContext;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<WorkPermitListViewModel> workPermitList = await GetWorkPermitList(search, Employee);
            workPermitList = SortWorkPermits(sort, workPermitList);
            var model = await LoadIndexViewModel(page, workPermitList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var workpermitQuery = await _workPermitServices.GetWorkPermitById(id);
            if (workpermitQuery == null)
            {
                return NotFound();
            }
            await _workPermitServices.DeleteWorkPermitAsync(workpermitQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted workpermit record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string Employee, string search)
        {
            Employee = Employee == "--- Select Employee ---" ? string.Empty : Employee;
            return RedirectToAction("index", new { Employee, search });
        }

        private async Task<PagedList<IEnumerable<WorkPermitListViewModel>, WorkPermitListViewModel>> LoadIndexViewModel(int? page, IEnumerable<WorkPermitListViewModel> workPermitList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = workPermitList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

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
        ///
        /// </summary>
        /// <param name="expiring"></param>
        /// <returns></returns>
        private async Task<IEnumerable<WorkPermitListViewModel>> GetWorkPermitList(string search, string Employee)
        {
            var workpermitQuery = await _workPermitServices.ListWorkPermitAsync();
            workpermitQuery = FilterWorkPermits(search, Employee, workpermitQuery);

            return workpermitQuery.Select(workpermit => new WorkPermitListViewModel
            {
                Id = workpermit.Id,
                WorkPermitNumber = workpermit.WorkPermitNumber,
                EmployeeId = workpermit.EmployeeId ?? Guid.Empty,
                Employee = workpermit.Employee.FullName,
                ExpiryDate = workpermit.ExpiryDate == null ? string.Empty : DateTime.Parse(workpermit.ExpiryDate.ToString()).ToString("dd MMM, yyyy")
            });
        }

        private IEnumerable<WorkPermitListViewModel> SortWorkPermits(string sort, IEnumerable<WorkPermitListViewModel> workPermitLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_workpermit" : "employee";
            switch (sort)
            {
                case "employee":
                    workPermitLists = workPermitLists.OrderByDescending(workpermit => workpermit.EmployeeId);
                    break;

                case "employee_workpermit":
                    workPermitLists = workPermitLists.OrderBy(workpermit => workpermit.EmployeeId);
                    break;

                default:
                    workPermitLists = workPermitLists.OrderBy(workpermit => workpermit.EmployeeId);
                    break;
            }
            return workPermitLists;
        }

        private IEnumerable<WorkPermit> FilterWorkPermits(string search, string Employee, IEnumerable<WorkPermit> workPermitQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                workPermitQuery = workPermitQuery.Where(workpermit => workpermit.WorkPermitNumber.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(Employee, out Guid EmployeeId))
            {
                workPermitQuery = workPermitQuery.Where(workpermit => workpermit.EmployeeId == EmployeeId);
                ViewData["employee"] = Employee;
            }
            return workPermitQuery;
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
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Add(NewWorkPermitViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //if (DateTime.Parse(formData.ExpiryDate) <= DateTimeOffset.Now)
                    //{
                    //    ModelState.AddModelError("WorkPermits", $"Employee workpermit Expiry Date {formData.ExpiryDate} can't be now or less than today, Please enter a valid workpermit expiry date greater than today.");
                    //}
                    //else if (DateTimeOffset.Now.Date.Day - DateTime.Parse(formData.ExpiryDate).Day <= 7)
                    //{
                    //    int days = DateTime.Parse(formData.ExpiryDate).Day - DateTimeOffset.Now.Date.Day;
                    //    ModelState.AddModelError("WorkPermits", $"Employee workpermit can't be expiring in {days} days, Please enter a valid date that expiries in 8 days and above .");

                    //}
                    //else
                    //{
                    bool bIfExist = false;
                    var q = from c in _db.WorkPermits where c.WorkPermitNumber == formData.WorkPermitNumber || c.EmployeeId == formData.EmployeeId select c;
                    try
                    {
                        q.ToList()[0].WorkPermitNumber.ToString();
                        q.ToList()[0].EmployeeId.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("WorkPermits", $"Can not register duplicate record {formData.WorkPermitNumber} or employee is already registered");
                    }
                    else
                    {
                        await _workPermitServices.AddWorkPermitAsync(new WorkPermit
                        {
                            EmployeeId = formData.EmployeeId,
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            WorkPermitNumber = formData.WorkPermitNumber,
                            UserAccount = User.Identity.Name,
                            ExpiryDate = DateTime.Parse(formData.ExpiryDate)
                        });
                        TempData["Message"] = "WorkPermit added successfully";
                        _logger.LogInformation($"Successfully added employee workpermit {formData.WorkPermitNumber} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("WorkPermit", $"Can not register record employee workpermit number. {formData.WorkPermitNumber} Contact IT ServiceDesk for support thank you.");
                _logger.LogError(
                   error,
                   $"FAIL: failed to register {formData.WorkPermitNumber}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
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
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var workPermitQuery = await _workPermitServices.GetWorkPermitById(id);
            if (workPermitQuery == null)
            {
                return NotFound();
            }
            var model = new WorkPermitDetailViewModel
            {
                EmployeeId = workPermitQuery.EmployeeId ?? Guid.Empty,
                WorkPermitNumber = workPermitQuery.WorkPermitNumber,
                Id = workPermitQuery.Id,
                ExpiryDate = workPermitQuery.ExpiryDate == null ? string.Empty : DateTime.Parse(workPermitQuery.ExpiryDate.ToString()).ToString("yyyy-MM-dd")
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
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(WorkPermitDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //if (DateTime.Parse(formData.ExpiryDate) <= DateTimeOffset.Now)
                    //{
                    //    ModelState.AddModelError("WorkPermits", $"Employee WorkPermit Expiry Date {formData.ExpiryDate} can't be now or less than today, Please enter a valid WorkPermit expiry date greater than today.");
                    //}
                    //else if (DateTimeOffset.Now.Date.Day - DateTime.Parse(formData.ExpiryDate).Day <= 7)
                    //{
                    //    int days = DateTime.Parse(formData.ExpiryDate).Day - DateTimeOffset.Now.Date.Day;
                    //    ModelState.AddModelError("WorkPermits", $"Employee WorkPermit can't be expiring in {days} days, Please enter a valid date that expiries in 8 days and above .");

                    //}
                    //else
                    //{
                    //bool bIfExist = false;
                    //var q = from c in _db.WorkPermits where c.WorkPermitNumber == formData.WorkPermitNumber select c;
                    //try
                    //{
                    //    q.ToList()[0].WorkPermitNumber.ToString();
                    //    bIfExist = true;
                    //}
                    //catch { }
                    //if (bIfExist == true)
                    //{
                    //    ModelState.AddModelError("WorkPermits", $"Can not register duplicate record. {formData.WorkPermitNumber} is already registered");
                    //}
                    //else
                    //{
                    await _workPermitServices.UpdateWorkPermitAsync(new WorkPermit
                    {
                        EmployeeId = formData.EmployeeId,
                        WorkPermitNumber = formData.WorkPermitNumber,
                        ExpiryDate = DateTime.Parse(formData.ExpiryDate),
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Successfully updated employee workpermit {formData.WorkPermitNumber} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("WorkPermit", $"Can not workpermit record. {formData.WorkPermitNumber} Contact IT ServiceDesk for support thank you.");
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee workpermit {formData.EmployeeId}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}