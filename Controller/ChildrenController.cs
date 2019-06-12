using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Children;
using HRCentral.Services.Employees;
using HRCentral.Services.BirthOrder;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Children;
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
    public class ChildrenController : Controller
    {
        private readonly IChildrenServices _childrenServices;
        private readonly IBirthOrderServices _birthOrderServices;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ChildrenController> _logger;
        private readonly IEmployeeServices _employeeServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="childrenServices"></param>
        /// <param name="applicationDbContext"></param>
        /// <param name="logger"></param>
        /// <param name="employeeServices"></param>
        public ChildrenController(IChildrenServices childrenServices,
            ApplicationDbContext applicationDbContext,
            ILogger<ChildrenController> logger,
            IEmployeeServices employeeServices,
            IBirthOrderServices birthOrderServices)
        {
            _childrenServices = childrenServices;
            _db = applicationDbContext;
            _logger = logger;
            _employeeServices = employeeServices;
            _birthOrderServices = birthOrderServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<ChildrenListViewModel> childrenList = await GetChildrenList(search, Employee);
            childrenList = SortChildren(sort, childrenList);
            var model = await LoadIndexViewModel(page, childrenList);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var childQuery = await _childrenServices.GetChildrenById(id);
            if (childQuery == null)
            {
                return NotFound();
            }
            await _childrenServices.DeleteChildrenAsync(childQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted employee children record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        private async Task<PagedList<IEnumerable<ChildrenListViewModel>, ChildrenListViewModel>> LoadIndexViewModel(int? page, IEnumerable<ChildrenListViewModel> childrenList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = childrenList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        /// <summary>
        /// Sorts queried records from a search
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="childrenList"></param>
        /// <returns></returns>
        private IEnumerable<ChildrenListViewModel> SortChildren(string sort, IEnumerable<ChildrenListViewModel> childrenList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_children" : "employee";
            switch (sort)
            {
                case "employee":
                    childrenList = childrenList.OrderByDescending(children => children.EmployeeId);
                    break;

                case "employee_children":
                    childrenList = childrenList.OrderBy(children => children.EmployeeId);
                    break;

                default:
                    childrenList = childrenList.OrderBy(children => children.EmployeeId);
                    break;
            }
            return childrenList;
        }
        /// <summary>
        /// Returns a list of all employee children
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <returns></returns>
        private async Task<IEnumerable<ChildrenListViewModel>> GetChildrenList(string search, string employee)
        {
            var childQuery = await _childrenServices.ListChildrenAsync();
            childQuery = FilterChildren(search, employee, childQuery);

            return childQuery.Select(childrenQuery => new ChildrenListViewModel
            {
                Id = childrenQuery.Id,
                Name = childrenQuery.Name,
                Employee = childrenQuery.Employee.FullName,
                Gender = childrenQuery.Gender,
                BirthOrder = childrenQuery.BirthOrder.BirthOrderName,
                BirthOrderId = childrenQuery.BirthOrderID ?? Guid.Empty,
                EmployeeId = childrenQuery.EmployeeId ?? Guid.Empty,
                BirthDate = childrenQuery.DateOfBirth == null ? string.Empty : DateTime.Parse(childrenQuery.DateOfBirth.ToString()).ToString("dd MMM, yyyy")
            });
        }
        /// <summary>
        /// Filters records that are queried from the database
        /// </summary>
        /// <param name="search"></param>
        /// <param name="employee"></param>
        /// <param name="childrenQuery"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeChildren> FilterChildren(string search, string employee, IEnumerable<EmployeeChildren> childQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                childQuery = childQuery.Where(children => children.Name.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                childQuery = childQuery.Where(children => children.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }
            return childQuery;
        }
        /// <summary>
        /// Loads all employees in the database
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
            ViewData["BirthOrders"] = (await _birthOrderServices.ListBirthOrdersAsync())
                                            .Select(birthorder => new SelectListItem
                                            {
                                                Text = birthorder.BirthOrder,
                                                Value = birthorder.Id.ToString()
                                            });
        }
        /// <summary>
        /// Search method
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
        /// Adds a new entity of the child class
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewChildrenViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var img = await Base64StringHandler.GetFormFileBase64StringAsync(formData.BirthImg);
                    //bool bIfExist = false;
                    //var q = from c in _db.EmployeeChildrens where c.Name == formData.Name || c.BirthCertificateImage == img select c;
                   //try
                   //{
                        //q.ToList()[0].Name.ToString();
                        //q.ToList()[0].BirthCertificateImage.ToString();
                        //bIfExist = true;
                    //}
                    //catch { }
                    //if (bIfExist == true)
                    //{
                        //ModelState.AddModelError("Children", $"Can not register duplicate record. {formData.Name} is already registered");
                   // }
                    
                    //else
                    //{
                        //var imageToStore = await Base64StringHandler.GetFormFileBase64StringAsync(formData.BirthImg);
                        await _childrenServices.AddChildrenAsync(new EmployeeChildren
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            Name = formData.Name,
                            DateOfBirth = DateTime.Parse(formData.BirthDate),
                            Gender = formData.Gender,
                            BirthOrderID = formData.BirthOrderId,
                            EmployeeId = formData.EmployeeId,
                            UserAccount = User.Identity.Name
                            //BirthCertificateImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.BirthImg)

                    });
                        TempData["Message"] = "Child added successfully";
                        _logger.LogInformation($"Success: successfully added employee child details {formData.Name} record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                  $"FAIL: failed to register child with names of {formData.Name}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Children", $"Failed to record employee child with names of {formData.Name}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// Update constructor
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var childrenQuery = await _childrenServices.GetChildrenById(id);
            if (childrenQuery == null)
            {
                return NotFound();
            }
            
            var model = new ChildrenDetailViewModel
            {
                Id = childrenQuery.Id,
                EmployeeId = childrenQuery.EmployeeId ?? Guid.Empty,
                Name = childrenQuery.Name,
                BirthOrderId = childrenQuery.BirthOrderID,
                Gender = childrenQuery.Gender,
               // BirthCertificateImage = childrenQuery.BirthCertificateImage,
                BirthDate = childrenQuery.DateOfBirth == null ? string.Empty : DateTime.Parse(childrenQuery.DateOfBirth.ToString()).ToString("yyyy-MM-dd")
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        /// <summary>
        /// Updates employee child records
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ChildrenDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                
                    await _childrenServices.UpdateChildrenAsync(new EmployeeChildren
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeId = formData.EmployeeId,
                        Name = formData.Name,
                        DateOfBirth = DateTime.Parse(formData.BirthDate),
                        Gender = formData.Gender,
                       BirthOrderID = formData.BirthOrderId,
                        UserAccount = User.Identity.Name,
                        //BirthCertificateImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.BirthCertificateImg),
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee child record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee child details {formData.Name}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Children", $"Failed to update employee child record. {formData.Name} Contact IT ServiceDesk for support thank you.");
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
