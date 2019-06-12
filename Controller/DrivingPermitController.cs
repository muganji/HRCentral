using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.DrivingPermits;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.DrivingPermits;
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
    public class DrivingPermitController : Controller
    {
        private readonly IDrivingPermitServices _drivingPermitServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DrivingPermitController> _logger;
        private readonly IEmployeeServices _employeeServices;

        public DrivingPermitController(IDrivingPermitServices drivingPermitServices, IEmployeeServices employeeServices, ILogger<DrivingPermitController> logger, ApplicationDbContext applicationDbContext)
        {
            _drivingPermitServices = drivingPermitServices;
            _employeeServices = employeeServices;
            _db = applicationDbContext;
            _logger = logger;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<DrivingPermitListViewModel> drivingPermitList = await GetDrivingPermitList(search, Employee);
            drivingPermitList = SortDrivingPermits(sort, drivingPermitList);
            var model = await LoadIndexViewModel(page, drivingPermitList);
            ViewData["page"] = page;
            return View(model);
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

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string search, string Employee)
        {
            return RedirectToAction("index", new { search, Employee });
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var drivingPermitQuery = await _drivingPermitServices.GetDrivingPermitById(id);
            if (drivingPermitQuery == null)
            {
                return NotFound();
            }
            await _drivingPermitServices.DeleteDrivingPermitAsync(drivingPermitQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted drivingpermit record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        private IEnumerable<DrivingPermitListViewModel> SortDrivingPermits(string sort, IEnumerable<DrivingPermitListViewModel> drivingPermitLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_drivingpermit" : "employee";
            switch (sort)
            {
                case "employee":
                    drivingPermitLists = drivingPermitLists.OrderByDescending(drivingPermit => drivingPermit.EmployeeId);
                    break;

                case "employee_drivingpermit":
                    drivingPermitLists = drivingPermitLists.OrderBy(drivingPermit => drivingPermit.EmployeeId);
                    break;

                default:
                    drivingPermitLists = drivingPermitLists.OrderBy(drivingPermit => drivingPermit.EmployeeId);
                    break;
            }
            return drivingPermitLists;
        }

        private async Task<PagedList<IEnumerable<DrivingPermitListViewModel>, DrivingPermitListViewModel>> LoadIndexViewModel(int? page, IEnumerable<DrivingPermitListViewModel> drivingPermitList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = drivingPermitList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        private async Task<IEnumerable<DrivingPermitListViewModel>> GetDrivingPermitList(string search, string Employee)
        {
            var drivingpermitQuery = await _drivingPermitServices.ListDrivingPermitsAsync();
            drivingpermitQuery = FilterDrivingPermit(search, Employee, drivingpermitQuery);

            return drivingpermitQuery.Select(drivingpermit => new DrivingPermitListViewModel
            {
                Id = drivingpermit.Id,
                DrivingPermitNumber = drivingpermit.DrivingPermitNumber,
                EmployeeId = drivingpermit.EmployeeId ?? Guid.Empty,
                Employee = drivingpermit.Employee.FullName,
                ExpiryDate = drivingpermit.ExpiryDate == null ? string.Empty : DateTime.Parse(drivingpermit.ExpiryDate.ToString()).ToString("dd MMM, yyyy")
            });
        }

        private IEnumerable<DrivingPermit> FilterDrivingPermit(string search, string Employee, IEnumerable<DrivingPermit> drivingpermitQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                drivingpermitQuery = drivingpermitQuery.Where(drivingpermit => drivingpermit.DrivingPermitNumber.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(Employee, out Guid EmployeeId))
            {
                drivingpermitQuery = drivingpermitQuery.Where(drivingpermit => drivingpermit.EmployeeId == EmployeeId);
                ViewData["employee"] = Employee;
            }
            return drivingpermitQuery;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewDrivingPermtViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //if (DateTime.Parse(formData.ExpiryDate) <= DateTimeOffset.Now)
                    //{
                    //    ModelState.AddModelError("DrivingPermits", $"Employee DrivingPermit Expiry Date {formData.ExpiryDate} can't be now or less than today, Please enter a valid driving permit expiry date greater than today.");
                    //}
                    //else if (DateTimeOffset.Now.Date.Day - DateTime.Parse(formData.ExpiryDate).Day <= 7)
                    //{
                    //    int days = DateTime.Parse(formData.ExpiryDate).Day - DateTimeOffset.Now.Date.Day;
                    //    ModelState.AddModelError("DrivingPermits", $"Employee DrivingPermit can't be expiring in {days} days, Please enter a valid date that expiries in 8 days and above .");

                    //}
                    //else
                    //{
                    bool bIfExist = false;
                    var q = from c in _db.DrivingPermits where c.DrivingPermitNumber == formData.DrivingPermitNumber || c.EmployeeId == formData.EmployeeId select c;
                    try
                    {
                        q.ToList()[0].DrivingPermitNumber.ToString();
                        q.ToList()[0].EmployeeId.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("DrivingPermit", $"Can not register duplicate record. {formData.DrivingPermitNumber} is already registered or Employee already has a driving permit.");
                    }
                    else
                    {
                        await _drivingPermitServices.AddDrivingPermitAsync(new DrivingPermit
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            DrivingPermitNumber = formData.DrivingPermitNumber,
                            ExpiryDate = DateTime.Parse(formData.ExpiryDate),
                            EmployeeId = formData.EmployeeId,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "DrivingPermit added successfully";
                        _logger.LogInformation($"Success: successfully added employee DrivingPermit {formData.DrivingPermitNumber} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register {formData.DrivingPermitNumber}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("DrivingPermit", $"Failed to record employee driving permit number {formData.DrivingPermitNumber}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var drivingPermitQuery = await _drivingPermitServices.GetDrivingPermitById(id);
            if (drivingPermitQuery == null)
            {
                return NotFound();
            }
            var model = new DrivingPermitDetailViewModel
            {
                Id = drivingPermitQuery.Id,
                EmployeeId = drivingPermitQuery.EmployeeId ?? Guid.Empty,
                DrivingPermitNumber = drivingPermitQuery.DrivingPermitNumber,
                ExpiryDate = drivingPermitQuery.ExpiryDate == null ? string.Empty : DateTime.Parse(drivingPermitQuery.ExpiryDate.ToString()).ToString("yyyy-MM-dd"),
            };
            await LoadSelectListsAsync();
            return View(model);
        }

       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
       // [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(DrivingPermitDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _drivingPermitServices.UpdateDrivingPermitAsync(new DrivingPermit
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeId,
                        DrivingPermitNumber = formData.DrivingPermitNumber,
                        ExpiryDate = DateTime.Parse(formData.ExpiryDate),
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee drivingpermit record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee driving permit {formData.DrivingPermitNumber}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("DrivingPermit", $"Failed to update driving permit record. {formData.DrivingPermitNumber} Contact IT ServiceDesk for support thank you.");
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