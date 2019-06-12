using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Leavedays;
using HRCentral.Services.Employeeleave;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.EmployeeLeave;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Sakura.AspNetCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class EmployeeLeaveController : Controller
    {
        private readonly ILeaveDayServices _leaveDayServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IEmployeeLeaveServices _employeeLeaveServices;
        private readonly ILogger<EmployeeLeaveController> _logger;
        private readonly ApplicationDbContext _applicationDbContext;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        /// <param name="leaveDayServices"></param>
        /// /// <param name="employeeLeaveServices"></param>
        public EmployeeLeaveController(
            ApplicationDbContext applicationDbContext,
            ILogger<EmployeeLeaveController> logger,
            IEmployeeServices employeeServices,
            ILeaveDayServices leaveDayServices,
            IEmployeeLeaveServices employeeLeaveServices)
        {
            _logger = logger;
            _employeeServices = employeeServices;
            _applicationDbContext = applicationDbContext;
            _leaveDayServices = leaveDayServices;
            _employeeLeaveServices = employeeLeaveServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employeeLeaveQuery = await _employeeLeaveServices.GetEmployeeLeaveById(id);
            if (employeeLeaveQuery == null)
            {
                return NotFound();
            }
            await _employeeLeaveServices.DeleteEmployeeLeaveAsync(employeeLeaveQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted employee leave record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index", "home");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors,ACL-HRCentralDatabase-Admins")]
        public IActionResult Delete()
        {
            return View();
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
            ViewData["LeaveDays"] = (await _leaveDayServices.ListLeaveDaysAsync())
                .Select(leavedays => new SelectListItem
                {
                    Text = leavedays.Title,
                    Value = leavedays.Id.ToString()
                });

        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(NewEmployeeLeaveViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeLeaveServices.AddEmployeeLeaveAsync(new EmployeeLeave
                    {
                        DateTimeAdded = DateTimeOffset.Now,
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeId,
                        LeaveTypeID = formData.LeaveTypeId,
                        ReplacementId = formData.ReplacementId,
                        DaysToTake = formData.DaystoTake,
                        EndDate = DateTime.Parse(formData.EndDate),
                        StartDate = DateTime.Parse(formData.StartDate),
                        RejectReason = formData.RejectReason,
                        UserAccount = User.Identity.Name,
                        LeaveStatus = "Running",
                    });
                    TempData["Message"] = "Successfully applied for leave.";
                    _logger.LogInformation($"Success: Employee with pfnumber {User.Identity.Name.Substring(4)} succeffully applied for a leave.");
                    return RedirectToAction("apply");
                }


            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                           error,
                           $"FAIL: failed to register {formData.EmployeeId} Education details. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Education", $"Failed to register record. {formData.EmployeeId} Contact IT ServiceDesk for support thank you.");
            }

            await LoadSelectListsAsync();

            return View(formData);
        }
        public async Task<IActionResult> Apply()
        {
            await LoadSelectListsAsync();
            return View();
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var employeeLeaveQuery = await _employeeLeaveServices.GetEmployeeLeaveById(id);

            if (employeeLeaveQuery == null)
            {
                return NotFound();
            }

            var model = new EmployeeLeaveDetailViewModel
            {
                Id = employeeLeaveQuery.Id,
                EmployeeId = employeeLeaveQuery.EmployeeId ?? Guid.Empty,
                LeaveTypeId = employeeLeaveQuery.LeaveTypeID ?? Guid.Empty,
                ReplacementId = employeeLeaveQuery.ReplacementId ?? Guid.Empty,
                RejectReason = employeeLeaveQuery.RejectReason,
                DaystoTake = employeeLeaveQuery.DaysToTake ?? 0,
                StartDate = employeeLeaveQuery.StartDate == null ? string.Empty : DateTime.Parse(employeeLeaveQuery.StartDate.ToString()).ToString("yyyy-MM-dd"),
                EndDate = employeeLeaveQuery.EndDate == null ? string.Empty : DateTime.Parse(employeeLeaveQuery.EndDate.ToString()).ToString("yyyy-MM-dd")

            };
            await LoadSelectListsAsync();
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(EmployeeLeaveDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeLeaveServices.UpdateEmployeeLeaveAsync(new EmployeeLeave
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Id = formData.Id,
                        EmployeeId = formData.EmployeeId,
                        LeaveTypeID = formData.LeaveTypeId,
                        ReplacementId = formData.ReplacementId,
                        DaysToTake = formData.DaystoTake,
                        EndDate = DateTime.Parse(formData.EndDate),
                        StartDate = DateTime.Parse(formData.StartDate),
                        RejectReason = formData.RejectReason,
                        UserAccount = User.Identity.Name
                    });

                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"SUCCESS: successfully updated employee leave application by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee leave details. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("EmployeeLeave", $"Failed to update Employee leave record. Contact IT ServiceDesk for support thank you.");
            }

            await LoadSelectListsAsync();

            return View(formData);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string Employee, string sort, string search, string status, string leavetype)
        {
            IEnumerable<EmployeeLeaveListViewModel> employeeLeaveLists = await GetEmployeeLeave(Employee, status, leavetype, search);
            employeeLeaveLists = SortEmployeeLeave(sort, employeeLeaveLists);
            var model = await LoadIndexViewModel(page, employeeLeaveLists);
            ViewData["page"] = page;
            return View(model);
        }

        private IEnumerable<EmployeeLeaveListViewModel> SortEmployeeLeave(string sort, IEnumerable<EmployeeLeaveListViewModel> employeeLeaveLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_leave" : "employee";
            ViewData["LeaveTypeSort"] = sort == "leavetype" ? "leave_type" : "leave";
            ViewData["LeaveStatusSort"] = sort == "leavestatus" ? "leave_status" : "status";
            switch (sort)
            {
                case "employee":
                    employeeLeaveLists = employeeLeaveLists.OrderByDescending(leave => leave.EmployeeId);
                    break;

                case "employee_leave":
                    employeeLeaveLists = employeeLeaveLists.OrderBy(leave => leave.EmployeeId);
                    break;
                case "leave":
                    employeeLeaveLists = employeeLeaveLists.OrderByDescending(leave => leave.LeaveTypeId);
                    break;
                case "leave_type":
                    employeeLeaveLists = employeeLeaveLists.OrderBy(leave => leave.LeaveTypeId);
                    break;
                case "status":
                    employeeLeaveLists = employeeLeaveLists.OrderByDescending(leave => leave.Status);
                    break;
                case "leave_status":
                    employeeLeaveLists = employeeLeaveLists.OrderBy(leave => leave.Status);
                    break;

                default:
                    employeeLeaveLists = employeeLeaveLists.OrderBy(leave => leave.EmployeeId);
                    break;
            }
            return employeeLeaveLists;
        }

        private async Task<PagedList<IEnumerable<EmployeeLeaveListViewModel>, EmployeeLeaveListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EmployeeLeaveListViewModel> employeeLeaveLists)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = employeeLeaveLists.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        private async Task<IEnumerable<EmployeeLeaveListViewModel>> GetEmployeeLeave(string employee, string status, string leavetype, string search)
        {
            var employeeLeaveQuery = await _employeeLeaveServices.ListEmployeeLeaveAsync();

            employeeLeaveQuery = FilterEmployeeLeave(search, employee, status, leavetype, employeeLeaveQuery);

            return employeeLeaveQuery.Select(EmployeeLeaveQuery => new EmployeeLeaveListViewModel
            {
                Id = EmployeeLeaveQuery.Id,
                EmployeeId = EmployeeLeaveQuery.EmployeeId ?? Guid.Empty,
                LeaveTypeId = EmployeeLeaveQuery.LeaveTypeID ?? Guid.Empty,
                ReplacementId = EmployeeLeaveQuery.ReplacementId ?? Guid.Empty,
                StartDate = EmployeeLeaveQuery.StartDate == null ? string.Empty : DateTime.Parse(EmployeeLeaveQuery.StartDate.ToString()).ToString("dd MMM, yyyy"),
                EndDate = EmployeeLeaveQuery.EndDate == null ? string.Empty : DateTime.Parse(EmployeeLeaveQuery.EndDate.ToString()).ToString("dd MMM, yyyy"),
                Status = EmployeeLeaveQuery.LeaveStatus,
                Employee = EmployeeLeaveQuery.Employee.FullName,



            });
        }

        private IEnumerable<EmployeeLeave> FilterEmployeeLeave(string search, string employee, string status, string leavetype, IEnumerable<EmployeeLeave> employeeLeaveQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                employeeLeaveQuery = employeeLeaveQuery.Where(leave => leave.DaysToTake.ToString().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                employeeLeaveQuery = employeeLeaveQuery.Where(leave => leave.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }
            if (Guid.TryParse(leavetype, out Guid leaveId))
            {
                employeeLeaveQuery = employeeLeaveQuery.Where(leave => leave.LeaveTypeID == leaveId);
                ViewData["leavetype"] = leavetype;
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                employeeLeaveQuery = employeeLeaveQuery.Where(leave => leave.LeaveStatus == status.ToString());
                ViewData["status"] = status;
            }
            return employeeLeaveQuery;
        }
    }
}