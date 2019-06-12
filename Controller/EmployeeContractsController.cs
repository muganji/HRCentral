using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.MedicalCovers;
using HRCentral.Services.Contracts;
using HRCentral.Services.EmployeeContracts;
using HRCentral.Services.Employees;
using HRCentral.Web.Models;
using HRCentral.Web.Models.EmployeeContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Sakura.AspNetCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class EmployeeContractsController : Controller
    {
        private readonly IContractsServices _contractsServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IMedicalCoverServices _medicalCoverServices;
        private readonly IEmployeeContractServices _employeeContractServices;
        private readonly ILogger<EmployeeContractsController> _logger;
        private readonly ApplicationDbContext _db;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="contractsServices"></param>
        /// <param name="employeeServices"></param>
        /// <param name="medicalCoverServices"></param>
        /// <param name="logger"></param>
        public EmployeeContractsController(
            IContractsServices contractsServices,
            IEmployeeServices employeeServices,
            IMedicalCoverServices medicalCoverServices,
            ILogger<EmployeeContractsController> logger,
           ApplicationDbContext db,
           IEmployeeContractServices employeeContractServices)
        {
            _contractsServices = contractsServices;
            _employeeServices = employeeServices;
            _medicalCoverServices = medicalCoverServices;
            _logger = logger;
            _db = db;
            _employeeContractServices = employeeContractServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Index(int? page, string search, string Employee, string sort)
        {
            IEnumerable<EmployeeContractListViewModel> contractList = await GetContractsList(search, Employee);
            contractList = SortContracts(sort, contractList);
            var model = await LoadIndexViewModel(page, contractList);
            ViewData["page"] = page;
            return View(model);
        }
        private async Task<PagedList<IEnumerable<EmployeeContractListViewModel>, EmployeeContractListViewModel>> LoadIndexViewModel(int? page, IEnumerable<EmployeeContractListViewModel> contractList)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var model = contractList.ToPagedList(pageSize, pageNumber);

            await LoadSelectListsAsync();
            return model;
        }
        private IEnumerable<EmployeeContractListViewModel> SortContracts(string sort, IEnumerable<EmployeeContractListViewModel> contractList)
        {
            ViewData["EmployeeSort"] = sort == "employee" ? "employee_contract" : "employee";
            switch (sort)
            {
                case "employee":
                    contractList = contractList.OrderByDescending(contract => contract.EmployeeId);
                    break;

                case "employee_contract":
                    contractList = contractList.OrderBy(contract => contract.EmployeeId);
                    break;

                default:
                    contractList = contractList.OrderBy(contract => contract.EmployeeId);
                    break;
            }
            return contractList;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employeeContractQuery = await _employeeContractServices.GetEmployeeContractsById(id);
            if (employeeContractQuery == null)
            {
                return NotFound();
            }
            await _employeeContractServices.DeleteEmployeeContractAsync(employeeContractQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted employee contract record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        private async Task<IEnumerable<EmployeeContractListViewModel>> GetContractsList(string search, string employee)
        {
            var contractQuery = await _employeeContractServices.ListEmployeeContractsAsync();
            contractQuery = FilterContracts(search, employee, contractQuery);

            return contractQuery.Select(contractsQuery => new EmployeeContractListViewModel
            {
                Id = contractsQuery.Id,
                Employee = contractsQuery.Employee.FullName,
                Duration = contractsQuery.Contract.Duration,
               BasicPay = contractsQuery.BasicPay,
               TerminationReason = contractsQuery.TerminationReason,
               ContactID = contractsQuery.ContractId ?? Guid.Empty,
               WebAccess = contractsQuery.WebAccess,
               Mail = contractsQuery.Mail,
               Internet = contractsQuery.Internet,
               DailUpAccess = contractsQuery.DailUpAccess,
               Ability = contractsQuery.Ability,
               Finance = contractsQuery.Finance,
              ContractStatus = contractsQuery.ContractStatus,
               CDRInterConnection = contractsQuery.CDRInterConnection,
               AirtimeAllocation = contractsQuery.AirtimeAllocation,
               MedicalCoverId = contractsQuery.MedicalCoverID ?? Guid.Empty,
                EmployeeId = contractsQuery.EmployeeID ?? Guid.Empty,
                TerminationDate = contractsQuery.TerminationDate == null ? string.Empty : DateTime.Parse(contractsQuery.TerminationDate.ToString()).ToString("dd MMM, yyyy"),
                StartDate = contractsQuery.StartDate == null ? string.Empty : DateTime.Parse(contractsQuery.StartDate.ToString()).ToString("dd MMM, yyyy")
            });
        }
        private IEnumerable<EmployeeContract> FilterContracts(string search, string employee, IEnumerable<EmployeeContract> contractQuery)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                contractQuery = contractQuery.Where(contract => contract.AirtimeAllocation.ToLower().Contains(search.ToLower()));
                ViewData["search"] = search;
            }
            if (Guid.TryParse(employee, out Guid EmployeeId))
            {
                contractQuery = contractQuery.Where(contract => contract.EmployeeID == EmployeeId);
                ViewData["employee"] = employee;
            }
            return contractQuery;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        [HttpPost]
        public IActionResult Index(string search, string Employee)
        {
            return RedirectToAction("index", new { search, Employee });
        }
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Add()
        {
            await LoadSelectListsAsync();
            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(NewEmployeeContractViewModel formData)
        {
            try
            {
                
                if (ModelState.IsValid)
                {
                    if (
                        DateTime.Parse(formData.StartDate) > DateTimeOffset.Now)
                    {
                        ModelState.AddModelError("EmployeeContract", $"Start date {formData.StartDate} is invalid, Please enter a valid Start and termination date.");
                    }
                    else
                    {
                        int q = (from c in _db.Contracts where c.Id == formData.ContactID select c.Month).First();
                        DateTime ExpiryDate = DateTime.Parse(formData.StartDate).AddMonths(q);
                        await _employeeContractServices.AddEmployeeContractAsync(new EmployeeContract
                        {
                            EmployeeID = formData.EmployeeId,
                            StartDate = DateTime.Parse(formData.StartDate),
                            TerminationDate = ExpiryDate,
                            ContractId = formData.ContactID,
                            WebAccess = formData.WebAccess,
                            Mail = formData.Mail,
                            TerminationReason = formData.TerminationReason,
                            Internet = formData.Internet,
                            DailUpAccess = formData.DailUpAccess,
                            Ability = formData.Ability,
                            Finance = formData.Finance,
                            CDRInterConnection = formData.CDRInterConnection,
                            AirtimeAllocation = formData.AirtimeAllocation,
                            MedicalCoverID = formData.MedicalCoverId,
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                            ContractStatus = formData.ContractStatus
                        });
                        TempData["Message"] = "Employee Contract added successfully";
                        _logger.LogInformation($"Success: successfully added employee contract details record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }

            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employee contract record. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Contract", $"Failed to record employee contract record. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
        public async Task<IActionResult> Details(Guid Id)
        {
            var employeeContractQuery = await _employeeContractServices.GetEmployeeContractsById(Id);
            if (employeeContractQuery == null)
            {
                return NotFound();
            }
            var model = new EmployeeContractsDetailViewModel
            {
                Id = employeeContractQuery.Id,
                
                TerminationDate = employeeContractQuery.TerminationDate == null ? string.Empty : DateTime.Parse(employeeContractQuery.TerminationDate.ToString()).ToString("yyyy-MM-dd"),
                StartDate = employeeContractQuery.StartDate == null ? string.Empty : DateTime.Parse(employeeContractQuery.StartDate.ToString()).ToString("yyyy-MM-dd"),
                MedicalCoverId = employeeContractQuery.MedicalCoverID ?? Guid.Empty,
                EmployeeId = employeeContractQuery.EmployeeID ?? Guid.Empty,
                ContactID = employeeContractQuery.ContractId ?? Guid.Empty,
                TerminationReason = employeeContractQuery.TerminationReason,
                WebAccess = employeeContractQuery.WebAccess,
                Mail = employeeContractQuery.Mail,
                Internet = employeeContractQuery.Internet,
                DailUpAccess = employeeContractQuery.DailUpAccess,
                Ability = employeeContractQuery.Ability,
                Finance = employeeContractQuery.Finance,
                ContractStatus = employeeContractQuery.ContractStatus,
                CDRInterConnection = employeeContractQuery.CDRInterConnection,
                AirtimeAllocation = employeeContractQuery.AirtimeAllocation




            };
            await LoadSelectListsAsync();
            return View(model);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(EmployeeContractsDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _employeeContractServices.UpdateEmployeeContractAsync(new EmployeeContract
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        EmployeeID = formData.EmployeeId,
                        ContractId = formData.ContactID,
                        MedicalCoverID = formData.MedicalCoverId,
                        TerminationReason = formData.TerminationReason,
                        StartDate = DateTime.Parse(formData.StartDate),
                        TerminationDate = DateTime.Parse(formData.TerminationDate),
                        WebAccess = formData.WebAccess,
                        Mail = formData.Mail,
                        Internet = formData.Internet,
                        DailUpAccess = formData.DailUpAccess,
                        Ability = formData.Ability,
                        Finance = formData.Finance,
                        CDRInterConnection = formData.CDRInterConnection,
                        AirtimeAllocation = formData.AirtimeAllocation,
                        UserAccount = User.Identity.Name,
                        ContractStatus = formData.ContractStatus,
                        Id = formData.Id
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated employee contract record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
               
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee contract details. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Contract", $"Failed to update employee contract record. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        /// <summary>
        /// Loads data
        /// </summary>
        /// <returns></returns>
        private async Task LoadSelectListsAsync()
        {
            ViewData["EmployeeSelectList"] = (await _employeeServices.ListEmployeesAsync())
                .Where(emp => emp.IsActive)
                .Select(employee => new SelectListItem
                {
                    Text = employee.FirstName + ',' + ' ' + employee.LastName,
                    Value = employee.Id.ToString()
                });
            ViewData["MedicalCoverSelectList"] = (await _medicalCoverServices.ListMedicalCoversAsync())
                .Select(medical => new SelectListItem
                {
                    Text = medical.Title,
                    Value = medical.Id.ToString()
                });
            ViewData["ContractsSelectList"] = (await _contractsServices.ListContractsAsync())
                .Select(contracts => new SelectListItem
                {
                    Text = contracts.Title,
                    Value = contracts.Id.ToString()
                });
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}