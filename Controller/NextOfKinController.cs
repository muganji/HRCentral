using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Employees;
using HRCentral.Services.RelationshipTypes;
using HRCentral.Services.NextOfKins;
using HRCentral.Web.Models;
using HRCentral.Web.Models.NextOfKins;
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
    public class NextofkinController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<NextofkinController> _logger;
        private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly INextOfKinServices _nextOfKinServices;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="relationshipTypeServices"></param>
        /// <param name="employeeServices"></param>
        /// <param name="logger"></param>
        public NextofkinController(ApplicationDbContext applicationDbContext, IRelationshipTypeServices relationshipTypeServices,IEmployeeServices employeeServices, ILogger<NextofkinController> logger, INextOfKinServices nextOfKinServices)
        {
            _db = applicationDbContext;
            _logger = logger;
            _relationshipTypeServices = relationshipTypeServices;
            _employeeServices = employeeServices;
            _nextOfKinServices = nextOfKinServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string Employee, string sort, string search)
        {
            IEnumerable<NextOfKinListViewModel> nextofkinLists = await GetNextofKinList(Employee, search);
            nextofkinLists = SortNextofKins(sort, nextofkinLists);
            var model = await LoadIndexViewModel(page, nextofkinLists);
            ViewData["page"] = page;
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var nextofkinQuery = await _nextOfKinServices.GetNextOfKinById(id);
            if (nextofkinQuery == null)
            {
                return NotFound();
            }
            await _nextOfKinServices.DeleteNextOfKinAsync(nextofkinQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted nextofkin record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        private IEnumerable<NextOfKinListViewModel> SortNextofKins(string sort, IEnumerable<NextOfKinListViewModel> nextofkinLists)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_nextofkin" : "employee";
            switch (sort)
            {
                case "employee":
                    nextofkinLists = nextofkinLists.OrderByDescending(nextofkin => nextofkin.EmployeeId);
                    break;

                case "employee_nextofkin":
                    nextofkinLists = nextofkinLists.OrderBy(nextofkin => nextofkin.EmployeeId);
                    break;

                default:
                    nextofkinLists = nextofkinLists.OrderBy(nextofkin => nextofkin.EmployeeId);
                    break;
            }
            return nextofkinLists;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string Employee, string search)
        {
            Employee = Employee == "--- Select Employee ---" ? string.Empty : Employee;
            return RedirectToAction("index", new { Employee, search });
        }
        private async Task<PagedList<IEnumerable<NextOfKinListViewModel>, NextOfKinListViewModel>> LoadIndexViewModel(int? page, IEnumerable<NextOfKinListViewModel> nextOfKinLists)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = nextOfKinLists.ToPagedList(pageSize, pageNumber);

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
            ViewData["Relationships"] = (await _relationshipTypeServices.ListRelationshipsAsync())
                                            .Select(relations => new SelectListItem
                                            {
                                                Text = relations.Title,
                                                Value = relations.Id.ToString()
                                            });
        }

        private async Task<IEnumerable<NextOfKinListViewModel>> GetNextofKinList(string Employee, string search)
        {
            var nexofKinQuery = await _nextOfKinServices.ListNextOfKinsAsync();

            nexofKinQuery = FilterNextofKins(search, Employee, nexofKinQuery);

            return nexofKinQuery.Select(NexofKinQuery => new NextOfKinListViewModel
            {
                Id = NexofKinQuery.Id,
                Name = NexofKinQuery.Name,
                Employee = NexofKinQuery.Employee.FullName,
               // Address = NexofKinQuery.Address,
                //Residence = NexofKinQuery.Residence,
                ContactInfo = NexofKinQuery.ContactInfo,
                RelationshipId = NexofKinQuery.RelationshipId ?? Guid.Empty,
                RelationShip = NexofKinQuery.RelationShipType.RelationshipName
                
            });
        }

        private IEnumerable<NextOfKin> FilterNextofKins(string search, string employee, IEnumerable<NextOfKin> nexofKinQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                nexofKinQuery = nexofKinQuery.Where(nextofkin => nextofkin.Name.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                nexofKinQuery = nexofKinQuery.Where(contact => contact.EmployeeId == EmployeeId);
                ViewData["employee"] = employee;
            }

            return nexofKinQuery;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid id)
        {
            var nextofkinQuery = await _nextOfKinServices.GetNextOfKinById(id);

            if (nextofkinQuery == null)
            {
                return NotFound();
            }

            var model = new NextOfKinDetailViewModel
            {
              Name = nextofkinQuery.Name,
             // Address = nextofkinQuery.Address,
              //Residence = nextofkinQuery.Residence,
              ContactInfo = nextofkinQuery.ContactInfo,
              RelationshipId = nextofkinQuery.RelationshipId,
                Id = nextofkinQuery.Id,
                EmployeeId = nextofkinQuery.EmployeeId ?? Guid.Empty
            };
            await LoadSelectListsAsync();
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(NextOfKinDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                await _nextOfKinServices.UpdateNextOfKinAsync(new NextOfKin
                        {
                            DateTimeModified = DateTimeOffset.Now,

                            RelationshipId = formData.RelationshipId,
                            Name = formData.Name,
                            //Address = formData.Address,
                            //Residence = formData.Residence,
                            ContactInfo = formData.ContactInfo,
                            EmployeeId = formData.EmployeeId,
                            Id = formData.Id,
                            UserAccount = User.Identity.Name
                        });

                        TempData["Message"] = "Changes saved successfully";
                        _logger.LogInformation($"SUCCESS: successfully updated employee {formData.EmployeeId} nextofkin record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("details", new { id = formData.Id });
                    }
                
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update {formData.EmployeeId} nextofkin. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("NextofKin", $"Failed to update record {formData.EmployeeId}. Contact IT ServiceDesk for support thank you.");
            }

            await LoadSelectListsAsync();

            return View(formData);
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
        public async Task<IActionResult> Add(NewNextOfKinViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var w = from c in _db.NextOfKins
                            where c.Name == formData.Name
                            select c;
                    try
                    {
                        w.ToList()[0].Name.ToString();
                       
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("NextofKin", "Can not register duplicate record. Employee nextofkin already exists.");
                    }
                    
                    else
                    {
                       await _nextOfKinServices.AddNextOfkinAsync(new NextOfKin
                            {
                                DateTimeAdded = DateTimeOffset.Now,
                                DateTimeModified = DateTimeOffset.Now,

                           RelationshipId = formData.RelationshipId,
                           Name = formData.Name,
                           //Address = formData.Address,
                           //Residence = formData.Residence,
                           ContactInfo = formData.ContactInfo,

                                EmployeeId = formData.EmployeeId,
                                UserAccount = User.Identity.Name
                            });
                            TempData["Message"] = "NextofKin added successfully";
                            _logger.LogInformation($"Success: successfully added employee {formData.EmployeeId} nextofkin record by user={@User.Identity.Name.Substring(4)}");
                            return RedirectToAction("add");
                        }
                    }
                
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                           error,
                           $"FAIL: failed to register {formData.EmployeeId} Nextofkin. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("NextofKin", $"Failed to register record. {formData.EmployeeId} Contact IT ServiceDesk for support thank you.");
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