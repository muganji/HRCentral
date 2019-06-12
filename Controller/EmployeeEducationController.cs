using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.EducationDetails;
using HRCentral.Services.Employees;
using HRCentral.Services.Qualifications;
using HRCentral.Web.Models;
using HRCentral.Web.Models.EducationDetails;
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
using HRCentral.Core.Base64;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class EmployeeEducationController : Controller
    {
        private readonly IEducationDetailServices _educationDetailServices;
        private readonly ApplicationDbContext _db;
        private readonly IEmployeeServices _employeeServices;
        private readonly IQualificationServices _qualificationServices;
      
        private readonly ILogger<EmployeeEducationController> _logger;

        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="educationDetailServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        public EmployeeEducationController(
            IEducationDetailServices educationDetailServices,
            ApplicationDbContext applicationDbContext,
            ILogger<EmployeeEducationController> logger,
            IEmployeeServices employeeServices,
            IQualificationServices qualificationServices)
            
        {
            _educationDetailServices = educationDetailServices;
            _db = applicationDbContext;
            _logger = logger;
        
            _employeeServices = employeeServices;
            _qualificationServices = qualificationServices;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var educationQuery = await _educationDetailServices.GetEducationDetailById(id);
            if (educationQuery == null)
            {
                return NotFound();
            }
            await _educationDetailServices.DeleteEducationAsync(educationQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted education record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
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
            ViewData["Qualifications"] = (await _qualificationServices.ListQualificationsAsync())
                .Select(qualification => new SelectListItem
                {
                    Text = qualification.Title,
                    Value = qualification.Id.ToString()
                });
           
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <param name="Employee"></param>
        /// <param name="sort"></param>
        /// <param name="search"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string Employee, string sort, string search, string Qualification)
        {
            IEnumerable<EducationDetailListViewModel> employeeEducationLists = await GetEmployeeEducation(Employee, Qualification, search);
            employeeEducationLists = SortEmployeeEducations(sort, employeeEducationLists);
            var model = await LoadIndexViewModel(page, employeeEducationLists);
            ViewData["page"] = page;
            return View(model);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="employee"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EducationDetailListViewModel>> GetEmployeeEducation(string employee, string Qualification, string search)
        {
            var employeeEducationQuery = await _educationDetailServices.ListEducationDetailsAsync();

            employeeEducationQuery = FilterEmployeeEducation(search, employee, Qualification, employeeEducationQuery);

            return employeeEducationQuery.Select(EmployeeEducationQuery => new EducationDetailListViewModel
            {
                Id = EmployeeEducationQuery.Id,
                Institution = EmployeeEducationQuery.Institution,
                EmployeeId = EmployeeEducationQuery.EmployeeId ?? Guid.Empty,
                Begin = EmployeeEducationQuery.From == null ? string.Empty : Int32.Parse(EmployeeEducationQuery.From.ToString()).ToString(),
                End = EmployeeEducationQuery.To == null ? string.Empty : Int32.Parse(EmployeeEducationQuery.To.ToString()).ToString(),
                Status = EmployeeEducationQuery.Status,
                Employee = EmployeeEducationQuery.Employee.FullName,
                QualificationTitle = EmployeeEducationQuery.QualificationTitle,
                Qualification = EmployeeEducationQuery.Qualification.QualificationName,
                QualificationId = EmployeeEducationQuery.QualificationId
                

            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="employeeEducationQuery"></param>
        /// <returns></returns>
        private IEnumerable<EducationalDetails> FilterEmployeeEducation(string search, string employee, string Qualification, IEnumerable<EducationalDetails> employeeEducationQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                employeeEducationQuery = employeeEducationQuery.Where(education => education.Institution.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                employeeEducationQuery = employeeEducationQuery.Where(education => education.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }
            if (Guid.TryParse(Qualification, out Guid QualificationId))
            {
                employeeEducationQuery = employeeEducationQuery.Where(education => education.QualificationId == QualificationId);
                ViewData["qualification"] = Qualification;
            }
            
            return employeeEducationQuery;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Employee"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string Employee, string Qualification, string search)
        {
            Employee = Employee == "--- Select Employee ---" ? string.Empty : Employee;
            Qualification = Qualification == "--- Select Qualification ---" ? string.Empty : Qualification;
            
            return RedirectToAction("index", new { Employee, Qualification, search });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <param name="employeeEducationLists"></param>
        /// <returns></returns>
        private async Task<PagedList<IEnumerable<EducationDetailListViewModel>, EducationDetailListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EducationDetailListViewModel> employeeEducationLists)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = employeeEducationLists.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="employeeEducationLists"></param>
        /// <returns></returns>
        private IEnumerable<EducationDetailListViewModel> SortEmployeeEducations(string sort, IEnumerable<EducationDetailListViewModel> employeeEducationLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_education" : "employee";
            switch (sort)
            {
                case "employee":
                    employeeEducationLists = employeeEducationLists.OrderByDescending(education => education.EmployeeId);
                    break;

                case "employee_education":
                    employeeEducationLists = employeeEducationLists.OrderBy(education => education.EmployeeId);
                    break;

                default:
                    employeeEducationLists = employeeEducationLists.OrderBy(education => education.EmployeeId);
                    break;
            }
            return employeeEducationLists;
        }

       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var employeeEducationQuery = await _educationDetailServices.GetEducationDetailById(id);

            if (employeeEducationQuery == null)
            {
                return NotFound();
            }

            var model = new EducationDetailViewModel
            {
                Id = employeeEducationQuery.Id,
                Institution = employeeEducationQuery.Institution,
                EmployeeId = employeeEducationQuery.EmployeeId ?? Guid.Empty,
                Begin = employeeEducationQuery.From,
                End = employeeEducationQuery.To,
                Status = employeeEducationQuery.Status,
                QualificationTitle = employeeEducationQuery.QualificationTitle,
                QualificationId = employeeEducationQuery.QualificationId
                //DocumentFile = employeeEducationQuery.DocumentImage

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
        public async Task<IActionResult> Details(EducationDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _educationDetailServices.UpdateDEducationDetailAsync(new EducationalDetails
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Id = formData.Id,
                        Institution = formData.Institution,
                        EmployeeId = formData.EmployeeId,
                        Status = formData.Status,
                        From = formData.Begin,
                        To = formData.End,
                        QualificationTitle = formData.QualificationTitle,
                        QualificationId = formData.QualificationId,
                        //DocumentImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.DocumentImg),
                        UserAccount = User.Identity.Name
                    });

                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"SUCCESS: successfully updated employee {formData.EmployeeId} education record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update {formData.EmployeeId} education detail. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Education", $"Failed to update record {formData.EmployeeId}. Contact IT ServiceDesk for support thank you.");
            }

            await LoadSelectListsAsync();

            return View(formData);
        }
      
        /// <summary>
        ///
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
       [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewEducationViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                   
                        //}
                        //else
                        //{
                        //var img = await Base64StringHandler.GetFormFileBase64StringAsync(formData.DocumentImg);
                        //bool bIfExist = false;
                        //var emp = from c in _db.EducationalDetails
                        //where c.DocumentImage == img
                        //select c;
                        //try
                        //{
                        //emp.ToList()[0].DocumentImage.ToString();
                        //bIfExist = true;
                        //}
                        // catch { }
                        //if (bIfExist == true)
                        //{
                        //ModelState.AddModelError("Documents", $"Education document already exists in the database.");

                        //}
                        //else
                        //{
                        await _educationDetailServices.AddEducationDetailsAsync(new EducationalDetails
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Institution = formData.Institution,
                            EmployeeId = formData.EmployeeId,

                            Status = formData.Status,
                            From = formData.Begin,
                            To = formData.End,
                            QualificationTitle = formData.QualificationTitle,
                            QualificationId = formData.QualificationId,
                            //DocumentImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.DocumentImg),
                            UserAccount = User.Identity.Name
                        });
                        TempData["Message"] = "Education details added successfully";
                        _logger.LogInformation($"Success: successfully added employee {formData.EmployeeId} education record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}