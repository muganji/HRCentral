using HRCentral.Core.Models;
using HRCentral.Services.Districts;
using HRCentral.Services.Employees;
//using HRCentral.Services.RelationshipTypes;
using HRCentral.Services.Banks;
using HRCentral.Services.Departments;
using HRCentral.Services.Nationalities;
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
    public class RetiredController : Controller
    {
        private readonly IDistrictServices _districtServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly ILogger<HomeController> _logger;
        //private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly IBankServices _bankServices;
        private readonly INationalityServices _nationalityServices;

        public RetiredController(

            IDistrictServices districtServices,
            IEmployeeServices employeeServices,
            IDepartmentServices departmentServices,
            ILogger<HomeController> logger,
            IBankServices bankServices,
             INationalityServices nationalityServices
              //IRelationshipTypeServices relationshipTypeServices

           )
        {
            _districtServices = districtServices;
            _employeeServices = employeeServices;
            _departmentServices = departmentServices;
            _logger = logger;
            //_relationshipTypeServices = relationshipTypeServices;
            _nationalityServices = nationalityServices;
            _bankServices = bankServices;

        }

        public async Task<IActionResult> Index(int? page, string search, string District, string Gender, string Department, string sort)
        {
            _logger.LogInformation($"Loading data from the database by user={@User.Identity.Name.Substring(4)}");
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
            _logger.LogInformation($"Querying database records by a filter, user={@User.Identity.Name.Substring(4)}");

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
            ViewData["ManagerSelectList"] = (await _employeeServices.ListEmployeesAsync())
                .Where(emp => emp.IsActive)
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
           
            ViewData["NationalitySelectList"] = (await _nationalityServices.ListNationalitiesAsync())
                .Select(nationality => new SelectListItem
                {
                    Text = nationality.Description,
                    Value = nationality.Id.ToString()
                });
   
            ViewData["BankSelectList"] = (await _bankServices.ListBankAsync())
               .Select(bank => new SelectListItem
               {
                   Text = bank.Name,
                   Value = bank.Id.ToString()
               });
            //ViewData["RelationshipSelectList"] = (await _relationshipTypeServices.ListRelationshipsAsync())
            //   .Select(relationship => new SelectListItem
            //   {
            //       Text = relationship.Title,
            //       Value = relationship.Id.ToString()
            //   });
        }

        private async Task<IEnumerable<EmployeeListViewModel>> GetEmployeeList(string search, string District, string Gender, string Department)
        {
            var employeeQuery = await _employeeServices.ListEmployeesAsync();
            employeeQuery = FilterEmployee(search, District, Gender, Department, employeeQuery);
            return employeeQuery.Select(employee => new EmployeeListViewModel
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
            }).Where(employee => !employee.IsActive);
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid Id)
        {

            var employeeQuery = await _employeeServices.GetEmployeeById(Id);
            if (employeeQuery == null)
            {
                return NotFound();
            }
            var model = new EmployeeDetailViewModel
            {
                Id = employeeQuery.Id,
                PFNumber = employeeQuery.PersonnelFileNumber,
                Salutation = employeeQuery.Salutation,
                FirstName = employeeQuery.FirstName,
                MiddleName = employeeQuery.MiddleName,
                MaidenName = employeeQuery.MaidenName,
                LastName = employeeQuery.LastName,
                DateOfBirth = employeeQuery.DateOfBirth == null ? string.Empty : DateTime.Parse(employeeQuery.DateOfBirth.ToString()).ToString("yyyy-MM-dd"),
                Gender = employeeQuery.Gender,
                NationalityId = employeeQuery.NationalityId ?? Guid.Empty,
                Religion = employeeQuery.Religion,
                MaritalStatus = employeeQuery.MaritalStatus,
                Address = employeeQuery.Address,
                NINNumber = employeeQuery.NationalIDNumber,
                TINNumber = employeeQuery.TINNumber,
                NSSFNumber = employeeQuery.NSSFNumber,
                DistrictBirthId = employeeQuery.DistrictBirthId ?? Guid.Empty,
                DistrictResidenceId = employeeQuery.DistrictResidenceId ?? Guid.Empty,
                SupervisorId = employeeQuery.SupervisorId ?? Guid.Empty,
                JobTitle = employeeQuery.JobTitle,
                ContactPerson = employeeQuery.ContactPerson,
                ContactPersonTelephone = employeeQuery.ContactPersonTelphone,
                IsActive = employeeQuery.IsActive,
                DepartmentId = employeeQuery.DepartmentId ?? Guid.Empty,
                BankId = employeeQuery.BankId ?? Guid.Empty,
                BankBranch = employeeQuery.BankBranch,
                BankAccountNumber = employeeQuery.BankAccountNumber,
                //BeneficiaryName = employeeQuery.BeneficiaryName,
                //BeneficiaryContact = employeeQuery.BeneficiaryContact,
                //RelationshipId = employeeQuery.RelationshipId,
                PersonalMobileNumber = employeeQuery.PersonalMobileNumber,
                AlternativeMobileNumber = employeeQuery.AlternativeMobileNumber,
                WorkMobileNumber = employeeQuery.WorkMobileNumber,
                SpouseName = employeeQuery.SpouseName,
                SpouseContactAddress = employeeQuery.SpouseContactAddress,
                SpouseDateOfBirth = employeeQuery.SpouseDateOfBirth == null ? string.Empty : DateTime.Parse(employeeQuery.SpouseDateOfBirth.ToString()).ToString("yyyy-MM-dd"),
                SpouseTelephone = employeeQuery.SpouseTelephone




            };
            await LoadSelectListsAsync();
            return View(model);
        }

    }
}