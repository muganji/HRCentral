using HRCentral.Core.Models;
using HRCentral.Services.Districts;
using HRCentral.Services.Employees;
using HRCentral.Services.Departments;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Employees;
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
    public class HomeController : Controller
    {
        private readonly IDistrictServices _districtServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly ILogger<HomeController> _logger;



        public HomeController(

            IDistrictServices districtServices,
            IEmployeeServices employeeServices,
            IDepartmentServices departmentServices,
            ILogger<HomeController> logger
           )
        {
            _districtServices = districtServices;
            _employeeServices = employeeServices;
            _departmentServices = departmentServices;
            _logger = logger;

        }

        public async Task<IActionResult> Index(int? page, string search, string District, string Gender, string Department, string sort)
        {
           // _logger.LogInformation($"Loading data from the database by user={@User.Identity.Name.Substring(4)}");
            IEnumerable<EmployeeListViewModel> employeeList = await GetEmployeeList(search, District, Gender, Department);
            employeeList = SortEmployees(sort, employeeList);
            var model = await LoadIndexViewModel(page, employeeList);
            ViewData["page"] = page;
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public IActionResult Index(string search, string District, string Gender, string Department)
        {
            //_logger.LogInformation($"Querying database records by a filter, user={@User.Identity.Name.Substring(4)}");

            District = District == "--- Select District ---" ? string.Empty : District;

            Department = Department == "--- Select Department ---" ? string.Empty : Department;

            Gender = Gender == "--- Select Gender ---" ? string.Empty : Gender;


            return RedirectToAction("index", new { search, District, Gender, Department });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sort"></param>
        /// <param name="employeeList"></param>
        /// <returns></returns>
        private IEnumerable<EmployeeListViewModel> SortEmployees(string sort, IEnumerable<EmployeeListViewModel> employeeList)
        {

            ViewData["DistrictSort"] = sort == "district" ? "employee_district" : "district";

            ViewData["GenderSort"] = sort == "gender" ? "employee_gender" : "gender";

            ViewData["DepartmentSort"] = sort == "department" ? "employee_department" : "department";
            switch (sort)
            {
                case "employee_district":
                    employeeList = employeeList.OrderBy(employee => employee.DistrictBirthId);
                    break;

                case "district":
                    employeeList = employeeList.OrderByDescending(employee => employee.DistrictBirthId);
                    break;
                case "gender":
                    employeeList = employeeList.OrderByDescending(employee => employee.Gender);
                    break;
                case "employee_gender":
                    employeeList = employeeList.OrderBy(employee => employee.Gender);
                    break;

                case "department":
                    employeeList = employeeList.OrderBy(employee => employee.DepartmentId);
                    break;

                case "employee_department":
                    employeeList = employeeList.OrderByDescending(employee => employee.DepartmentId);
                    break;

                default:
                    employeeList = employeeList.OrderBy(employee => employee.IsActive);
                    break;
            }
            return employeeList;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="page"></param>
        /// <param name="employeeList"></param>
        /// <returns></returns>
        private async Task<PagedList<IEnumerable<EmployeeListViewModel>, EmployeeListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EmployeeListViewModel> employeeList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = employeeList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private async Task LoadSelectListsAsync()
        {
            ViewData["DistrictSelectList"] = (await _districtServices.ListDistrictsAsync())
                .Select(district => new SelectListItem
                {
                    Text = district.Name,
                    Value = district.Id.ToString()
                });
            ViewData["EmployeeSelectList"] = (await _employeeServices.ListEmployeesAsync())
                .Select(employee => new SelectListItem
                {
                    Text = employee.FirstName + ',' + ' ' + employee.LastName,
                    Value = employee.Id.ToString()
                });

            ViewData["DepartmentSelectList"] = (await _departmentServices.ListDepartmentsAsync())
                .Select(department => new SelectListItem
                {
                    Text = department.Title,
                    Value = department.Id.ToString()
                });
        }

        private async Task<IEnumerable<EmployeeListViewModel>> GetEmployeeList(string search, string District, string Gender, string Department)
        {
            var employeeQuery = await _employeeServices.ListEmployeesAsync();
            employeeQuery = FilterEmployee(search, District, Gender, Department, employeeQuery);
            return employeeQuery.Where(employee => employee.IsActive)
                .Select(employee => new EmployeeListViewModel
            {
                Id = employee.Id,
                PersonnelFileNumber = employee.PersonnelFileNumber,
                Salutation = employee.Salutation,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                JobTitle = employee.JobTitle,
                ContactPerson = employee.ContactPerson,
                HomeAddress = employee.Address,
                AccountNumber = employee.BankAccountNumber,
                AlternativeMobileNumber = employee.AlternativeMobileNumber,
                PersonalMobileNumber = employee.PersonalMobileNumber,
                IsActive = employee.IsActive,
                DateOfBirth = employee.DateOfBirth == null ? string.Empty : DateTime.Parse(employee.DateOfBirth.ToString()).ToString("yyyy-MM-dd"),
                BankName = employee.Bank.Name,
                Department = employee.Department.DepartmentName,
                District = employee.DistrictResidence.DistrictName,
                Nationality = employee.Nationality.NationalityName
            });
        }

        /// <summary>

        private IEnumerable<Employee> FilterEmployee(string search, string District, string Gender, string Department, IEnumerable<Employee> employeeQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                employeeQuery = employeeQuery.Where(employee => employee.PersonnelFileNumber.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }

            if (!string.IsNullOrWhiteSpace(Gender))
            {
                employeeQuery = employeeQuery.Where(employee => employee.Gender == Gender.ToString());
                ViewData["gender"] = Gender;
            }

            if (Guid.TryParse(District, out Guid DistrictId))
            {
                employeeQuery = employeeQuery.Where(employee => employee.DistrictBirth.Id == DistrictId);
                ViewData["district"] = District;
            }

            if (Guid.TryParse(Department, out Guid DepartmentId))
            {
                employeeQuery = employeeQuery.Where(employee => employee.Department.Id == DepartmentId);
                ViewData["department"] = Department;
            }
            return employeeQuery;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return NotFound();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return NotFound();
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            return NotFound();
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