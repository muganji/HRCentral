using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Emails;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Email;
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
    public class EmailController : Controller
    {
        private readonly INotificationEmailServices _notificationEmailServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<EmailController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        /// <param name="notificationEmailServices"></param>
        public EmailController(

            ApplicationDbContext applicationDbContext,
            ILogger<EmailController> logger,
            IEmployeeServices employeeServices,
            INotificationEmailServices notificationEmailServices)
        {
            _db = applicationDbContext;
            _logger = logger;
            _employeeServices = employeeServices;
            _notificationEmailServices = notificationEmailServices;
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
            IEnumerable<EmailListViewModel> emailList = await GetEmailsList(search, Employee);
            emailList = SortEmails(sort, emailList);
            var model = await LoadIndexViewModel(page, emailList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var emailNotificationQuery = await _notificationEmailServices.GetEmailDetailById(id);
            if (emailNotificationQuery == null)
            {
                return NotFound();
            }
            await _notificationEmailServices.DeleteNotificationEmailAsync(emailNotificationQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted email address record by user={@User.Identity.Name.Substring(4)}");
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
        /// <param name="page"></param>
        /// <param name="emailList"></param>
        /// <returns></returns>
        private async Task<PagedList<IEnumerable<EmailListViewModel>, EmailListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EmailListViewModel> emailList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = emailList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="emailList"></param>
        /// <returns></returns>
        private IEnumerable<EmailListViewModel> SortEmails(string sort, IEnumerable<EmailListViewModel> emailList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_email" : "employee";
            switch (sort)
            {
                case "employee":
                    emailList = emailList.OrderByDescending(email => email.EmployeeId);
                    break;

                case "employee_beneficiary":
                    emailList = emailList.OrderBy(email => email.EmployeeId);
                    break;

                default:
                    emailList = emailList.OrderBy(email => email.EmployeeId);
                    break;
            }
            return emailList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task<IEnumerable<EmailListViewModel>> GetEmailsList(string search, string employee)
        {
            var emailQuery = await _notificationEmailServices.ListEmailDetailsAsync();
            emailQuery = FilterEmails(search, employee, emailQuery);

            return emailQuery.Select(emailsQuery => new EmailListViewModel
            {
                Id = emailsQuery.Id,
                EmailAddress = emailsQuery.EmailAddress,
                Employee = emailsQuery.Employee.FullName,
                EmployeeId = emailsQuery.EmployeeID ?? Guid.Empty,
                DateAdded = emailsQuery.DateTimeAdded == null ? string.Empty : DateTime.Parse(emailsQuery.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                DateModified = emailsQuery.DateTimeModified == null ? string.Empty : DateTime.Parse(emailsQuery.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                CreatedBy = emailsQuery.UserAccount

            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="emailQuery"></param>
        /// <returns></returns>
        private IEnumerable<NotificationEmails> FilterEmails(string search, string employee, IEnumerable<NotificationEmails> emailQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                emailQuery = emailQuery.Where(emails => emails.EmailAddress.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                emailQuery = emailQuery.Where(beneficiary => beneficiary.EmployeeID == EmployeeId);
                ViewData["employee"] = employee;
            }
            return emailQuery;
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
           
        }
        /// <summary>
        /// 
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
        public async Task<IActionResult> Add(NewEmailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    bool bIfExist = false;
                    var q = from c in _db.NotificationEmails where c.EmailAddress == formData.EmailAddress || c.EmployeeID == formData.EmployeeId select c;
                    try
                    {
                        q.ToList()[0].EmployeeID.ToString();
                        q.ToList()[0].EmailAddress.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Emails", $"Can not register duplicate record. {formData.EmailAddress} or {formData.EmployeeId} is already registered");
                    }

                    else
                    {
                        await _notificationEmailServices.AddEmailDetailsAsync(new NotificationEmails
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            EmailAddress = formData.EmailAddress,
                            EmployeeID = formData.EmployeeId,
                            UserAccount = User.Identity.Name


                        });
                        TempData["Message"] = "Notification Email added successfully";
                        _logger.LogInformation($"Success: successfully added notification email details {formData.EmailAddress} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                  $"FAIL: failed to register notification email with address of {formData.EmailAddress}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Email", $"Failed to record notification email with address of {formData.EmailAddress}. Contact IT ServiceDesk for support thank you.");
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
        public async Task<IActionResult> Details(Guid id)
        {
            var notificationQuery = await _notificationEmailServices.GetEmailDetailById(id);
            if (notificationQuery == null)
            {
                return NotFound();
            }

            var model = new EmailDetailViewModel
            {
                Id = notificationQuery.Id,
                EmployeeId = notificationQuery.EmployeeID ?? Guid.Empty,
                EmailAddress = notificationQuery.EmailAddress
         

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
        public async Task<IActionResult> Details(EmailDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _notificationEmailServices.UpdateEmailDetailAsync(new NotificationEmails
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeID = formData.EmployeeId,
                         EmailAddress = formData.EmailAddress,
                        UserAccount = User.Identity.Name,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated notification email record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update notification email details {formData.EmailAddress}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Beneficiary", $"Failed to update notification email record. {formData.EmailAddress} Contact IT ServiceDesk for support thank you.");
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