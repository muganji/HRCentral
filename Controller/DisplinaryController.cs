using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Displinaries;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Displinaries;
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
    public class DisplinaryController : Controller
    {
        private readonly  IDisplinaryServices  _displinaryServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DisplinaryController> _logger;
        private readonly IEmployeeServices _employeeServices;
        public DisplinaryController(
            IDisplinaryServices displinaryServices,
            ApplicationDbContext applicationDbContext,
            ILogger<DisplinaryController> logger,
            IEmployeeServices employeeServices)
        {
            _displinaryServices = displinaryServices;
            _db = applicationDbContext;
            _logger = logger;
            _employeeServices = employeeServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<DisplinaryListViewModel> misConductList = await GetMisConductList(search, Employee);
            misConductList = SortMisConduct(sort, misConductList);
            var model = await LoadIndexViewModel(page, misConductList);
            ViewData["page"] = page;
            return View(model);
        }
        private async Task<PagedList<IEnumerable<DisplinaryListViewModel>, DisplinaryListViewModel>> LoadIndexViewModel(int? page, IEnumerable<DisplinaryListViewModel> misConductList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = misConductList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var displinaryQuery = await _displinaryServices.GetDisplinaryRecordById(id);
            if (displinaryQuery == null)
            {
                return NotFound();
            }
            await _displinaryServices.DeleteDisplinaryAsync(displinaryQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted displinary record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        private IEnumerable<DisplinaryListViewModel> SortMisConduct(string sort, IEnumerable<DisplinaryListViewModel> misConductList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_misconduct" : "employee";
            switch (sort)
            {
                case "employee":
                    misConductList = misConductList.OrderByDescending(misconduct => misconduct.EmployeeID);
                    break;

                case "employee_misconduct":
                    misConductList = misConductList.OrderBy(misconduct => misconduct.EmployeeID);
                    break;

                default:
                    misConductList = misConductList.OrderBy(misconduct => misconduct.EmployeeID);
                    break;
            }
            return misConductList;
        }
        private async Task<IEnumerable<DisplinaryListViewModel>> GetMisConductList(string search, string employee)
        {
            var misconductQuery = await _displinaryServices.ListDisplinaryRecordsAsync();
            misconductQuery = FilterMisConduct(search, employee, misconductQuery);

            return misconductQuery.Select(displinaryQuery => new DisplinaryListViewModel
            {
                Id = displinaryQuery.Id,
                CrimeDescription = displinaryQuery.CrimeDescription,
                SentenceImposed = displinaryQuery.SentenceImposed,
                Employee = displinaryQuery.Employee.FullName,
                IsConvicted = displinaryQuery.IsConvicted,
                ConvictionPlace = displinaryQuery.ConvictionPlace,
                EmployeeID = displinaryQuery.EmployeeID ?? Guid.Empty,
                ConvictionDate = displinaryQuery.ConvictionDate == null ? string.Empty : DateTime.Parse(displinaryQuery.ConvictionDate.ToString()).ToString("dd MMM, yyyy")
            });
        }
        private IEnumerable<Displinary> FilterMisConduct(string search, string employee, IEnumerable<Displinary> misconductQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                misconductQuery = misconductQuery.Where(misconduct => misconduct.ConvictionPlace.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                misconductQuery = misconductQuery.Where(misconduct => misconduct.EmployeeID == EmployeeId);
                ViewData["employee"] = employee;
            }
            return misconductQuery;
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
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewDisplinaryViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                        await _displinaryServices.AddDisplinaryAsync(new Displinary
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            IsConvicted = formData.IsConvicted,
                            ConvictionDate = DateTime.Parse(formData.ConvictionDate),
                            ConvictionPlace = formData.ConvictionPlace,
                            SentenceImposed = formData.SentenceImposed,
                            CrimeDescription = formData.CrimeDescription,
                            EmployeeID = formData.Employee,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "MisConduct added successfully";
                        _logger.LogInformation($"Success: successfully added employee misconduct details {formData.ConvictionDate} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    

                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employee misconduct record of date {formData.ConvictionDate}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Crime", $"Failed to record employee misconduct record of date {formData.ConvictionDate}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var misconductQuery = await _displinaryServices.GetDisplinaryRecordById(id);
            if (misconductQuery == null)
            {
                return NotFound();
            }
            var model = new DisplinaryDetailsViewModel
            {
                Id = misconductQuery.Id,
                Employee = misconductQuery.EmployeeID ?? Guid.Empty,
                ConvictionPlace = misconductQuery.ConvictionPlace,
                IsConvicted = misconductQuery.IsConvicted,
                SentenceImposed = misconductQuery.SentenceImposed,
                CrimeDescription = misconductQuery.CrimeDescription,
                ConvictionDate = misconductQuery.ConvictionDate == null ? string.Empty : DateTime.Parse(misconductQuery.ConvictionDate.ToString()).ToString("yyyy-MM-dd")
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(DisplinaryDetailsViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    
                        await _displinaryServices.UpdateDisplinaryAsync(new Displinary
                        {
                            DateTimeModified = DateTimeOffset.Now,
                            EmployeeID = formData.Employee,
                            CrimeDescription = formData.CrimeDescription,
                            ConvictionDate = DateTime.Parse(formData.ConvictionDate),
                            ConvictionPlace = formData.ConvictionPlace,
                            IsConvicted = formData.IsConvicted,
                            SentenceImposed = formData.SentenceImposed,
                            UserAccount = User.Identity.Name,
                            Id = formData.Id
                        });
                        TempData["Message"] = "Changes saved successfully";
                        _logger.LogInformation($"Success: successfully updated employee misconduct record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("details", new { id = formData.Id });
                    
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee misconduct details {formData.ConvictionDate}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Children", $"Failed to update employee misconduct record. {formData.ConvictionDate} Contact IT ServiceDesk for support thank you.");
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