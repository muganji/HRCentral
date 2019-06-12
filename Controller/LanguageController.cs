using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRCentral.Core.Models;
using HRCentral.Services.Languages;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Languages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using HRCentral.Core.Data;
using System.Diagnostics;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    
    public class LanguageController : Controller
    {
        private readonly ILanguageServices _languageServices;
        private readonly ApplicationDbContext _db;
        private readonly IEmployeeServices _employeeServices;
        private readonly ILogger<LanguageController> _logger;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeServices"></param>
        /// <param name="languageServices"></param>
        public LanguageController(IEmployeeServices employeeServices, ILanguageServices languageServices, ILogger<LanguageController> logger, ApplicationDbContext applicationDbContext)
        {
            _languageServices = languageServices;
            _employeeServices = employeeServices;
            _logger = logger;
            _db = applicationDbContext;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="page"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string Employee, string sort, string search)
        {
            IEnumerable<LanguageListViewModel> LanguageLists = await GetLanguageList(Employee, search);
            LanguageLists = SortLanguages(sort, LanguageLists);
            var model = await LoadIndexViewModel(page, LanguageLists);
            ViewData["page"] = page;
            return View(model);

        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var languageQuery = await _languageServices.GetLanguageById(id);
            if (languageQuery == null)
            {
                return NotFound();
            }
            await _languageServices.DeleteLanguageAsync(languageQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted language record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        private async Task<IEnumerable<LanguageListViewModel>> GetLanguageList(string employee, string search)
        {
            var languageQuery = await _languageServices.ListLanguageAsync();

            languageQuery = FilterLanguage(search, employee, languageQuery);

            return languageQuery.Select(LanguageQuery => new LanguageListViewModel
            {
                Id = LanguageQuery.Id,
                SpeechProficiency = LanguageQuery.SpeechProficiency,
                EmployeeId = LanguageQuery.EmployeeId ?? Guid.Empty,
                WrittenProficiency = LanguageQuery.WrittenProficiency,
                ReadProficiency  = LanguageQuery.ReadProficiency,
                EmployeeName = LanguageQuery.Employee.FullName
                
            });
        }

        private async Task<PagedList<IEnumerable<LanguageListViewModel>, LanguageListViewModel>> LoadIndexViewModel(int? page, IEnumerable<LanguageListViewModel> languageLists)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = languageLists.ToPagedList(pageSize, pageNumber);

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

        

        [HttpPost]
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public IActionResult Index(string Employee, string search)
        {
            Employee = Employee == "--- Select Employee ---" ? string.Empty : Employee;
            return RedirectToAction("index", new { Employee, search });
        }
        private IEnumerable<Language> FilterLanguage(string search, string Employee, IEnumerable<Language> languageQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                languageQuery = languageQuery.Where(language => language.SpeechProficiency.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(Employee, out Guid EmployeeId))
            {
                languageQuery = languageQuery.Where(language => language.EmployeeId == EmployeeId);
                ViewData["employee"] = Employee;
            }
            return languageQuery;
        }
        private IEnumerable<LanguageListViewModel> SortLanguages(string sort, IEnumerable<LanguageListViewModel> languageLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_language" : "employee";
            switch (sort)
            {
                case "employee":
                    languageLists = languageLists.OrderByDescending(language => language.EmployeeId);
                    break;
                case "employee_language":
                    languageLists = languageLists.OrderBy(language => language.EmployeeId);
                    break;
                default:
                    languageLists = languageLists.OrderBy(language => language.EmployeeId);
                    break;
            }
            return languageLists;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            _logger.LogInformation($"Initializing the Edit Employee Language Action, user={@User.Identity.Name.Substring(4)}");
            var languageQuery = await _languageServices.GetLanguageById(id);
            if(languageQuery == null)
            {
                return NotFound();
            }
            var model = new LanguageDetailViewModel
            {
                Id = languageQuery.Id,
                WrittenProficiency = languageQuery.WrittenProficiency,
                SpeechProficiency = languageQuery.SpeechProficiency,
                ReadProficiency = languageQuery.ReadProficiency,
                EmployeeId = languageQuery.EmployeeId ?? Guid.Empty
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
        
        public async Task<IActionResult> Details(LanguageDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _languageServices.UpdateLanguageAsync(new Language
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeId,
                        WrittenProficiency = formData.WrittenProficiency,
                        SpeechProficiency = formData.SpeechProficiency,
                        ReadProficiency = formData.ReadProficiency,
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Successfully updated employee language {formData.SpeechProficiency + ' ' + formData.WrittenProficiency} record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });

                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee language details {formData.EmployeeId}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Language", $"Failed to update employee language record {formData.SpeechProficiency + ' ' + formData.WrittenProficiency}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            _logger.LogInformation($"Initializing the Add Employee Language Action, user={@User.Identity.Name.Substring(4)}");
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
        
        public async Task<IActionResult> Add(NewLanguageViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Languages where c.EmployeeId == formData.EmployeeId select c;
                    try
                    {
                        q.ToList()[0].EmployeeId.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist==true)
                    {
                        ModelState.AddModelError("Langugae", $"Can not register duplicate record. { formData.EmployeeId} is already registered with language details");
                    }
                    else
                    {
                        await _languageServices.AddLanguageAsync(new Language
                        {
                            EmployeeId = formData.EmployeeId ?? Guid.Empty,
                            WrittenProficiency = formData.WrittenProficiency,
                            SpeechProficiency = formData.SpeechProficiency,
                            ReadProficiency = formData.ReadProficiency,
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Language added successfully";
                        _logger.LogInformation($"Successfully added employee language {formData.SpeechProficiency + ' ' + formData.WrittenProficiency} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to record employee language {formData.EmployeeId}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Language", $"Failed to register employee language {formData.SpeechProficiency + ' ' + formData.WrittenProficiency}. Contact IT ServiceDesk for support thank you.");
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