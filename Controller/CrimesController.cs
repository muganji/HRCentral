using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.EmployeeCrimes;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Crimes;
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
    public class CrimesController : Controller
    {
        private readonly ICrimeServices _crimeServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CrimesController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="crimeServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        public CrimesController(
            ICrimeServices crimeServices,
            ApplicationDbContext applicationDbContext,
            ILogger<CrimesController> logger,
            IEmployeeServices employeeServices)
        {
            _crimeServices = crimeServices;
            _db = applicationDbContext;
            _logger = logger;
            _employeeServices = employeeServices;
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
            IEnumerable<CrimeListViewModel> crimeList = await GetCrimeList(search, Employee);
            crimeList = SortCrimes(sort, crimeList);
            var model = await LoadIndexViewModel(page, crimeList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var crimeQuery = await _crimeServices.GetCrimeById(id);
            if (crimeQuery == null)
            {
                return NotFound();
            }
            await _crimeServices.DeleteCrimesAsync(crimeQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted crime record by user={@User.Identity.Name.Substring(4)}");
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
        /// <param name="page"></param>
        /// <param name="crimeList"></param>
        /// <returns></returns>
        private async Task<PagedList<IEnumerable<CrimeListViewModel>, CrimeListViewModel>> LoadIndexViewModel(int? page, IEnumerable<CrimeListViewModel> crimeList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = crimeList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        /// <summary>
        /// Sorting function
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="crimeList"></param>
        /// <returns></returns>
        private IEnumerable<CrimeListViewModel> SortCrimes(string sort, IEnumerable<CrimeListViewModel> crimeList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_crime" : "employee";
            switch (sort)
            {
                case "employee":
                    crimeList = crimeList.OrderByDescending(crime => crime.EmployeeID);
                    break;

                case "employee_crime":
                    crimeList = crimeList.OrderBy(crime => crime.EmployeeID);
                    break;

                default:
                    crimeList = crimeList.OrderBy(crime => crime.EmployeeID);
                    break;
            }
            return crimeList;
        }
        /// <summary>
        /// Returns a list of all crimes
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task<IEnumerable<CrimeListViewModel>> GetCrimeList(string search, string employee)
        {
            var crimeQuery = await _crimeServices.ListCrimesAsync();
            crimeQuery = FilterCrime(search, employee, crimeQuery);

            return crimeQuery.Select(crimesQuery => new CrimeListViewModel
            {
                Id = crimesQuery.Id,
                CrimeDescription = crimesQuery.CrimeDescription,
                SentenceImposed = crimesQuery.SentenceImposed,
                Employee = crimesQuery.Employee.FullName,
                IsConvicted = crimesQuery.IsConvicted,
                ConvictionPlace = crimesQuery.ConvictionPlace,
                EmployeeID = crimesQuery.EmployeeID ?? Guid.Empty,
                ConvictionDate = crimesQuery.ConvictionDate == null ? string.Empty : DateTime.Parse(crimesQuery.ConvictionDate.ToString()).ToString("dd MMM, yyyy")
            });
        }
        /// <summary>
        /// Filters a returned list of crimes
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="crimeQuery"></param>
        /// <returns></returns>
        private IEnumerable<Crimes> FilterCrime(string search, string employee, IEnumerable<Crimes> crimeQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                crimeQuery = crimeQuery.Where(crime => crime.ConvictionPlace.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                crimeQuery = crimeQuery.Where(crime => crime.EmployeeID == EmployeeId);
                ViewData["employee"] = employee;
            }
            return crimeQuery;
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
        /// The Index Constructor
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
        /// Add Construtor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        /// <summary>
        /// Adds a new record in the crimes table
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewCriminalViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (DateTime.Parse(formData.ConvictionDate) >= DateTimeOffset.Now)
                    {
                        ModelState.AddModelError("Employees", $"Conviction Date {formData.ConvictionDate} can't be greater or now, Please enter a valid Conviction date.");
                    }
                    else
                    {
                        await _crimeServices.AddCrimeAsync(new Crimes
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
                        TempData["Message"] = "Crime added successfully";
                        _logger.LogInformation($"Success: successfully added employee criminal details {formData.ConvictionDate} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }

                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employee criminal record of date {formData.ConvictionDate}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Crime", $"Failed to record employee criminal record of date {formData.ConvictionDate}. Contact IT ServiceDesk for support thank you.");
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
            var crimeQuery = await _crimeServices.GetCrimeById(id);
            if (crimeQuery == null)
            {
                return NotFound();
            }
            var model = new CrimeDetailViewModel
            {
                Id = crimeQuery.Id,
                Employee = crimeQuery.EmployeeID ?? Guid.Empty,
                ConvictionPlace = crimeQuery.ConvictionPlace,
                IsConvicted = crimeQuery.IsConvicted,
                SentenceImposed = crimeQuery.SentenceImposed,
                CrimeDescription = crimeQuery.CrimeDescription,
                ConvictionDate = crimeQuery.ConvictionDate == null ? string.Empty : DateTime.Parse(crimeQuery.ConvictionDate.ToString()).ToString("yyyy-MM-dd")
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        /// <summary>
        /// Updates existing nodes on the T class.
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(CrimeDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (DateTime.Parse(formData.ConvictionDate) >= DateTimeOffset.Now)
                    {
                        ModelState.AddModelError("Employees", $"Conviction Date {formData.ConvictionDate} can't be greater or now, Please enter a valid Conviction date.");
                    }
                    else
                    {
                        await _crimeServices.UpdateCrimeAsync(new Crimes
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
                        _logger.LogInformation($"Success: successfully updated employee criminal record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("details", new { id = formData.Id });
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee criminal details {formData.ConvictionDate}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Children", $"Failed to update employee criminal record. {formData.ConvictionDate} Contact IT ServiceDesk for support thank you.");
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