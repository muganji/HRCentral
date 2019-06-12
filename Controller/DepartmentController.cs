using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Departments;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IDepartmentServices _departmentServices;
        private readonly ILogger<DepartmentController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="departmentServices"></param>
        /// <param name="logger"></param>
        public DepartmentController(ApplicationDbContext applicationDbContext,IDepartmentServices departmentServices,ILogger<DepartmentController> logger)
        {
            _db = applicationDbContext;
            _departmentServices = departmentServices;
            _logger = logger;
        }

        /// <summary>
        /// Returns a list of all departments
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var departments = (await _departmentServices.ListDepartmentsAsync())
                .Select(department => new DepartmentListViewModel
                {
                    Title = department.Title,
                    Id = department.Id,
                    DateAdded = department.DateTimeAdded == null ? string.Empty : DateTime.Parse(department.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = department.DateTimeModified == null ? string.Empty : DateTime.Parse(department.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = department.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(departments);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var departmentQuery = await _departmentServices.GetdepartmentById(id);
            if (departmentQuery == null)
            {
                return NotFound();
            }
            await _departmentServices.DeleteDepartmentAsync(departmentQuery);
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted department record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summ
        /// <summary>
        /// Initializes the update method.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var departmentQuery = await _departmentServices.GetdepartmentById(id);

            if (departmentQuery == null)
            {
                return NotFound();
            }

            var model = new DepartmentDetailViewModel
            {
                Title = departmentQuery.Title,
                Id = departmentQuery.Id,
            };

            return View(model);
        }
        /// <summary>
        /// Updates existing departments in the database
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(DepartmentDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _departmentServices.UpdateDepartment(new Department
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Title = formData.Title,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Title} department record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Bank", $"Failed to update record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Title} Department. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
            }

            return View(formData);
        }
        /// <summary>
        /// Initializes the add method
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Add()
        {
            return View();
        }
        /// <summary>
        /// Adds a new entity of the department class
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewDepartmentViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.Departments where c.Title == formData.Title select c;
                    try
                    {
                        q.ToList()[0].Title.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Department", $"Can not register duplicate record. {formData.Title} Department is already registered");
                    }
                    else
                    {
                        await _departmentServices.AddDepartmentAsync(new Department
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Title = formData.Title,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "Department Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Title} department record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Department", $"Failed to register record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Title} Department. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}