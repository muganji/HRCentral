using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Employees;
using HRCentral.Services.Visas;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Visas;
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
    //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class VisaController : Controller
    {
        private readonly IVisaServices _visaServices;
        private readonly ApplicationDbContext _db;
        private readonly IEmployeeServices _employeeServices;
        private readonly ILogger<VisaController> _logger;

        public VisaController(IVisaServices visaServices, IEmployeeServices employeeServices, ILogger<VisaController> logger, ApplicationDbContext applicationDbContext)
        {
            _visaServices = visaServices;
            _employeeServices = employeeServices;
            _logger = logger;
            _db = applicationDbContext;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<VisaListViewModel> visaList = await GetVisaList(search, Employee);
            visaList = SortVisas(sort, visaList);
            var model = await LoadIndexViewModel(page, visaList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var visaQuery = await _visaServices.GetVisaById(id);
            if (visaQuery == null)
            {
                return NotFound();
            }
            await _visaServices.DeleteVisaAsync(visaQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted visa record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
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
        }

        private IEnumerable<VisaListViewModel> SortVisas(string sort, IEnumerable<VisaListViewModel> visaLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_visa" : "employee";
            switch (sort)
            {
                case "employee":
                    visaLists = visaLists.OrderByDescending(visa => visa.EmployeeId);
                    break;

                case "employee_visa":
                    visaLists = visaLists.OrderBy(visa => visa.EmployeeId);
                    break;

                default:
                    visaLists = visaLists.OrderBy(visa => visa.EmployeeId);
                    break;
            }
            return visaLists;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string Employee, string search)
        {
            Employee = Employee == "--- Select Employee ---" ? string.Empty : Employee;
            return RedirectToAction("index", new { Employee, search });
        }

        private async Task<PagedList<IEnumerable<VisaListViewModel>, VisaListViewModel>> LoadIndexViewModel(int? page, IEnumerable<VisaListViewModel> visaList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = visaList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        private async Task<IEnumerable<VisaListViewModel>> GetVisaList(string search, string Employee)
        {
            var visaQuery = await _visaServices.ListVisasAsync();
            visaQuery = FilterVisas(search, Employee, visaQuery);

            return visaQuery.Select(visa => new VisaListViewModel
            {
                Id = visa.Id,
                VisaNumber = visa.VisaNumber,
                EmployeeId = visa.EmployeeId ?? Guid.Empty,
                Employee = visa.Employee.FullName,
                ExpiryDate = visa.ExpiryDate == null ? string.Empty : DateTime.Parse(visa.ExpiryDate.ToString()).ToString("dd MMM, yyyy")
            });
        }

        private IEnumerable<Visa> FilterVisas(string search, string Employee, IEnumerable<Visa> visaQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                visaQuery = visaQuery.Where(visa => visa.VisaNumber.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(Employee, out Guid EmployeeId))
            {
                visaQuery = visaQuery.Where(visa => visa.EmployeeId == EmployeeId);
                ViewData["employee"] = Employee;
            }
            return visaQuery;
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Add(NewVisaViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //if (DateTime.Parse(formData.ExpiryDate) <= DateTimeOffset.Now)
                    //{
                    //    ModelState.AddModelError("Visas", $"Employee Visa Expiry Date {formData.ExpiryDate} can't be now or less than today, Please enter a valid visa expiry date greater than today.");
                    //}
                    //else if (DateTimeOffset.Now.Date.Day - DateTime.Parse(formData.ExpiryDate).Day <= 7)
                    //{
                    //    int days = DateTime.Parse(formData.ExpiryDate).Day - DateTimeOffset.Now.Date.Day;
                    //    ModelState.AddModelError("Visas", $"Employee Visa can't be expiring in {days} days, Please enter a valid date that expiries in 8 days and above .");

                    //}
                    //else
                    //{
                    bool bIfExist = false;
                    var q = from c in _db.Visas where c.VisaNumber == formData.VisaNumber || c.EmployeeId == formData.EmployeeId select c;
                    try
                    {
                        q.ToList()[0].VisaNumber.ToString();
                        q.ToList()[0].EmployeeId.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Visas", $"Can not register duplicate record, {formData.VisaNumber} or employee is already registered");
                    }
                    else
                    {
                        await _visaServices.AddVisaAsync(new Visa
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            VisaNumber = formData.VisaNumber,
                            ExpiryDate = DateTime.Parse(formData.ExpiryDate),
                            EmployeeId = formData.EmployeeId,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Visa added successfully";
                        _logger.LogInformation($"Successfully added employee visa {formData.VisaNumber} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employee visa {formData.VisaNumber}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Visa", $"Failed to register employee visa {formData.VisaNumber}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }

        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Editors")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
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
            var visaQuery = await _visaServices.GetVisaById(id);
            if (visaQuery == null)
            {
                return NotFound();
            }
            var model = new VisaDetailViewModel
            {
                Id = visaQuery.Id,
                EmployeeId = visaQuery.EmployeeId ?? Guid.Empty,
                VisaNumber = visaQuery.VisaNumber,
                ExpiryDate = visaQuery.ExpiryDate == null ? string.Empty : DateTime.Parse(visaQuery.ExpiryDate.ToString()).ToString("yyyy-MM-dd")
            };
            await LoadSelectListsAsync();
            return View(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(VisaDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //if (DateTime.Parse(formData.ExpiryDate) <= DateTimeOffset.Now)
                    //{
                    //    ModelState.AddModelError("Visas", $"Employee Visa Expiry Date {formData.ExpiryDate} can't be now or less than today, Please enter a valid visa expiry date greater than today.");
                    //}
                    //else if (DateTimeOffset.Now.Date.Day - DateTime.Parse(formData.ExpiryDate).Day <= 7)
                    //{
                    //    int days = DateTime.Parse(formData.ExpiryDate).Day - DateTimeOffset.Now.Date.Day;
                    //    ModelState.AddModelError("Visas", $"Employee Visa can't be expiring in {days} days, Please enter a valid date that expiries in 8 days and above .");

                    //}
                    ////else
                    ////{
                    ////    bool bIfExist = false;
                    ////    var q = from c in _db.Visas where c.VisaNumber == formData.VisaNumber select c;
                    ////    try
                    ////    {
                    ////        q.ToList()[0].VisaNumber.ToString();
                    ////        bIfExist = true;
                    ////    }
                    ////    catch { }
                    ////    if (bIfExist == true)
                    ////    {
                    ////        ModelState.AddModelError("Visas", $"Can not register duplicate record. {formData.VisaNumber} is already registered");
                    ////    }
                    //    else
                    //    {
                    await _visaServices.UpdateVisaAsync(new Visa
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        VisaNumber = formData.VisaNumber,
                        EmployeeId = formData.EmployeeId,
                        UserAccount = User.Identity.Name,
                        ExpiryDate = DateTime.Parse(formData.ExpiryDate),
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Successfully updated employee visa {formData.EmployeeId} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee visa {formData.EmployeeId}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Visa", $"Failed to update employee visa record {formData.EmployeeId}. Contact IT ServiceDesk for support thank you.");
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