using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.Districts;
using HRCentral.Services.Employees;
//using HRCentral.Services.RelationshipTypes;
using HRCentral.Services.Banks;
using HRCentral.Services.Departments;
using HRCentral.Services.Nationalities;
using HRCentral.Web.Models;
using HRCentral.Web.Models.Employees;
using HRCentral.Web.Models.Image;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using HRCentral.Core.Base64;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins,ACL-HRCentralDatabase-Readers")]
    public class EmployeeController : Controller
    {
        private readonly IDistrictServices _districtServices;
        //private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly IBankServices _bankServices;
        private readonly IDepartmentServices _departmentServices;
        private readonly ApplicationDbContext _db;
        private readonly IEmployeeServices _employeeServices;
        private readonly ILogger<EmployeeController> _logger;
        private readonly INationalityServices _nationalityServices;

        /// <summary>
        ///
        /// </summary>
        /// <param name="districtServices"></param>
        /// <param name="employeeServices"></param>
        /// <param name="genderServices"></param>
        /// <param name="maritalStatusServices"></param>
        /// <param name="nationalityServices"></param>
        /// <param name="religionServices"></param>
        public EmployeeController(
            IDistrictServices districtServices,
            IBankServices bankServices,
            IDepartmentServices departmentServices,
            ApplicationDbContext applicationDbContext,
            //IRelationshipTypeServices relationshipTypeServices,
            IEmployeeServices employeeServices,
            ILogger<EmployeeController> logger,
            INationalityServices nationalityServices
        )
        {
            _districtServices = districtServices;
            _bankServices = bankServices;
            _departmentServices = departmentServices;
            _employeeServices = employeeServices;
            _nationalityServices = nationalityServices;
            _logger = logger;
            _db = applicationDbContext;
            //_relationshipTypeServices = relationshipTypeServices;
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
            var employeeQuery = await _employeeServices.GetEmployeeById(id);
            if (employeeQuery == null)
            {
                return NotFound();
            }
            await _employeeServices.DeleteEmployeeAsync(employeeQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted employee record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index","home");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
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
        public async Task<IActionResult> Add(NewEmployeeViewModel formData)
        {
            try
            {
                if (DateTime.Parse(formData.DateOfBirth) >= DateTimeOffset.Now)
                {
                    ModelState.AddModelError("Employees", $"Employee Date of Birth {formData.DateOfBirth} can't be greater or now, Please enter a valid birth date.");
                }
                else if (DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.DateOfBirth).Year <= 17)
                {
                    int years = DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.DateOfBirth).Year;
                    ModelState.AddModelError("Employees", $"Employee aged {years} years is below the required working age, Please enter a valid birth date in range of 18 - 54.");
                }
                else if (DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.DateOfBirth).Year >= 55)
                {
                    int years = DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.DateOfBirth).Year;
                    ModelState.AddModelError("Employees", $"Employee aged {years} years is at the retirement level, Please enter a valid birth date in range of 18 - 54.");
                }
                else if((formData.SpouseDateOfBirth != null) && DateTime.Parse(formData.SpouseDateOfBirth) >= DateTimeOffset.Now)
                {
                    ModelState.AddModelError("Employees", $"Employee Spouse Date of Birth {formData.SpouseDateOfBirth} can't be greater or now, Please enter a valid birth date.");
                }
                else if ((formData.SpouseDateOfBirth != null) && DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.SpouseDateOfBirth).Year <= 17)
                {
                    int years = DateTimeOffset.Now.Date.Year - DateTime.Parse(formData.SpouseDateOfBirth).Year;
                   ModelState.AddModelError("Employees", $"Employee Spouse aged {years} years is below the required required age, Please enter a valid birth date in range of 18 - 54.");
                }
                else
                {
                    var img = await Base64StringHandler.GetFormFileBase64StringAsync(formData.Img);
                    //var image = await Base64StringHandler.GetFormFileBase64StringAsync(formData.EmployeeBirthCertificate);
                    if (ModelState.IsValid)
                    {
                        bool bIfExist = false;
                        var emp = from c in _db.Employees
                                  where c.PersonnelFileNumber == formData.PFNumber ||
          c.TINNumber == formData.TINNumber ||
         c.NSSFNumber == formData.NSSFNumber ||
          c.NationalIDNumber == formData.NINNumber ||
           c.ContactPerson == formData.ContactPerson ||
           c.ContactPersonTelphone == formData.ContactPersonTelephone ||
           c.BankAccountNumber == formData.BankAccountNumber ||

           c.WorkMobileNumber == formData.WorkMobileNumber ||
           c.PersonalMobileNumber == formData.PersonalMobileNumber ||
           c.AlternativeMobileNumber == formData.AlternativeMobileNumber  ||
           c.EmployeeImage == img

                                  select c;

                        try
                        {
                            emp.ToList()[0].PersonnelFileNumber.ToString();
                            emp.ToList()[0].TINNumber.ToString();
                            emp.ToList()[0].NSSFNumber.ToString();
                            emp.ToList()[0].NationalIDNumber.ToString();
                            emp.ToList()[0].ContactPerson.ToString();
                            emp.ToList()[0].ContactPersonTelphone.ToString();
                            emp.ToList()[0].BankAccountNumber.ToString();
                            emp.ToList()[0].PersonalMobileNumber.ToString();
                            emp.ToList()[0].AlternativeMobileNumber.ToString();
                            emp.ToList()[0].WorkMobileNumber.ToString();
                            emp.ToList()[0].EmployeeImage.ToString();
                            bIfExist = true;
                        }
                        catch { }
                        if (bIfExist == true)
                        {
                            ModelState.AddModelError("Employees", $"Employee PFNumber, TIN, NSSF, NIN Number already exists, PF, TIN, NSSF and NIN Number must be Unique");
                            
                        }
                        else
                        {
                            var SpouseDate = "";
                            if (formData.SpouseDateOfBirth != null)
                            {
                                SpouseDate = formData.SpouseDateOfBirth;
                            }
                            else {
                                SpouseDate = DateTime.Now.ToString();
                            }
                           
                            //var img1 = await Base64StringHandler.GetFormFileBase64StringAsync(formData.EmployeeImg);
                            await _employeeServices.AddEmployeeAsync(new Employee
                            {
                                PersonnelFileNumber = formData.PFNumber,
                                Salutation = formData.Salutation,
                                FirstName = formData.FirstName,
                                MiddleName = formData.MiddleName,
                                MaidenName = formData.MaidenName,
                                LastName = formData.LastName,
                                DateOfBirth = DateTime.Parse(formData.DateOfBirth),
                                Gender = formData.Gender,
                                NationalityId = formData.NationalityId,
                                Religion = formData.Religion,
                                MaritalStatus = formData.MaritalStatus,
                               Address = formData.Address,
                                NationalIDNumber = formData.NINNumber,
                                TINNumber = formData.TINNumber,
                                NSSFNumber = formData.NSSFNumber,
                                DistrictBirthId = formData.DistrictBirthId,
                                DistrictResidenceId = formData.DistrictResidenceId,
                                SupervisorId = formData.SupervisorId,
                                JobTitle = formData.JobTitle,
                                ContactPerson = formData.ContactPerson,
                                ContactPersonTelphone = formData.ContactPersonTelephone,
                                IsActive = formData.IsActive,
                                DepartmentId = formData.DepartmentId,
                               BankId = formData.BankId,
                               BankBranch = formData.BankBranch,
                                BankAccountNumber = formData.BankAccountNumber,
                               // BeneficiaryName = formData.BeneficiaryName,
                                //BeneficiaryContact = formData.BeneficiaryContact,
                                //RelationshipId = formData.RelationshipId,
                                PersonalMobileNumber = formData.PersonalMobileNumber,
                                AlternativeMobileNumber = formData.AlternativeMobileNumber,
                                WorkMobileNumber = formData.WorkMobileNumber,
                                DateTimeAdded = DateTimeOffset.Now,
                                DateTimeModified = DateTimeOffset.Now,
                                UserAccount = User.Identity.Name,
                                SpouseName = formData.SpouseName,
                                SpouseContactAddress = formData.SpouseContactAddress,
                                SpouseDateOfBirth = DateTime.Parse(SpouseDate),
                                SpouseTelephone = formData.SpouseTelephone,
                                    EmployeeImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.Img),
                                OtherReligion = formData.OtherReligion,
                                OtherMaritialStatus = formData.MaritalStatusOthers,
                                JoinDate = DateTime.Parse(formData.DateOfJoin)
                                
                                    //SpouseBirthCertificateImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.SpouseBirthCertificate),
                                    // EmployeeBirthCertificateImage = await Base64StringHandler.GetFormFileBase64StringAsync(formData.EmployeeBirthCertificate)
                                });
                            _logger.LogInformation($"Successfully add new employee record with PFNumber: {formData.PFNumber}; user={@User.Identity.Name.Substring(4)}");
                            TempData["Message"] = "Employee added successfully";
                            return RedirectToAction("add");
                        }
                    }
                }
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to register employee {formData.FirstName + ' ' + formData.LastName}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Employee", $"Failed to register employee {formData.FirstName + ' ' + formData.LastName}. Contact IT ServiceDesk for support thank you.");
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
                DateOfJoin = employeeQuery.JoinDate == null ? string.Empty : DateTime.Parse(employeeQuery.JoinDate.ToString()).ToString("yyyy-MM-dd"),
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
                PersonalMobileNumber = employeeQuery.PersonalMobileNumber,
                AlternativeMobileNumber = employeeQuery.AlternativeMobileNumber,
                WorkMobileNumber = employeeQuery.WorkMobileNumber,
                SpouseName = employeeQuery.SpouseName,
                SpouseContactAddress = employeeQuery.SpouseContactAddress,
                SpouseDateOfBirth =employeeQuery.SpouseDateOfBirth == null ? string.Empty : DateTime.Parse(employeeQuery.SpouseDateOfBirth.ToString()).ToString("yyyy-MM-dd"),
                SpouseTelephone = employeeQuery.SpouseTelephone
                



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
        public async Task<IActionResult> Details(EmployeeDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                        await _employeeServices.UpdateEmployeeAsync(new Employee
                        {
                            //PersonnelFileNumber = formData.PFNumber,
                            //Salutation = formData.Salutation,
                            //FirstName = formData.FirstName,
                            //MiddleName = formData.MiddleName,
                            MaidenName = formData.MaidenName,
                            //LastName = formData.LastName,
                            //DateOfBirth = DateTime.Parse(formData.DateOfBirth),
                            //Gender = formData.Gender,
                            //NationalityId = formData.NationalityId,
                            Religion = formData.Religion,
                            MaritalStatus = formData.MaritalStatus,
                            Address = formData.Address,
                            NationalIDNumber = formData.NINNumber,
                            TINNumber = formData.TINNumber,
                            NSSFNumber = formData.NSSFNumber,
                            //DistrictBirthId = formData.DistrictBirthId,
                            //DistrictResidenceId = formData.DistrictResidenceId,
                            SupervisorId = formData.SupervisorId,
                            JobTitle = formData.JobTitle,
                            ContactPerson = formData.ContactPerson,
                            ContactPersonTelphone = formData.ContactPersonTelephone,
                            IsActive = formData.IsActive,
                            JoinDate = DateTime.Parse(formData.DateOfJoin),
                            DepartmentId = formData.DepartmentId,
                            BankId = formData.BankId,
                            BankBranch = formData.BankBranch,
                            BankAccountNumber = formData.BankAccountNumber,
                            
                            //PersonalMobileNumber = formData.PersonalMobileNumber,
                            //AlternativeMobileNumber = formData.AlternativeMobileNumber,
                            WorkMobileNumber = formData.WorkMobileNumber,
                            DateTimeAdded = DateTimeOffset.Now,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                            //SpouseName = formData.SpouseName,
                            //SpouseContactAddress = formData.SpouseContactAddress,
                            //SpouseDateOfBirth = DateTime.Parse(formData.SpouseDateOfBirth),
                            //SpouseTelephone = formData.SpouseTelephone,
                            Id = formData.Id
                        });
                        TempData["Message"] = "Changes saved successfully";
                        _logger.LogInformation($"Successfully updated employee record with PFNumber: {formData.PFNumber}; user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("details", new { id = formData.Id });
                    }
                
            }
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee {formData.FirstName + ' ' + formData.LastName}. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Employee", $"Failed to update employee {formData.FirstName + ' ' + formData.LastName}. Contact IT ServiceDesk for support thank you.");
            }
            await LoadSelectListsAsync();
            return View(formData);
        }
        public async Task<IActionResult> UploadImage(NewImageView formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var img = await Base64StringHandler.GetFormFileBase64StringAsync(formData.Img);
                    await _employeeServices.UpdateEmployeeAsync(new Employee
                    {
                        Id = formData.EmployeeID,
                        EmployeeImage = img,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Employee image successfully uploaded";
                    _logger.LogInformation($"Successfully updated employee image; user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("UploadImage");
                }
            }
            
            catch (ApplicationException error)
            {
                _logger.LogError(
                   error,
                   $"FAIL: failed to update employee image. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
                ModelState.AddModelError("Employee", $"Failed to update employee image. Contact IT ServiceDesk for support thank you.");
            }
    await LoadSelectListsAsync();
            return View(formData);
}

/// <summary>
///
/// </summary>
/// <returns></returns>
private async Task LoadSelectListsAsync()
        {
            ViewData["ManagerSelectList"] = (await _employeeServices.ListEmployeesAsync())
                .Where(emp => emp.IsActive)
                .Select(employee => new SelectListItem
                {
                    Text = employee.FirstName + ',' + ' ' + employee.LastName,
                    Value = employee.Id.ToString()
                });
            ViewData["DistrictSelectList"] = (await _districtServices.ListDistrictsAsync())
                .Select(district => new SelectListItem
                {
                    Text = district.Name,
                    Value = district.Id.ToString()
                });
            ViewData["NationalitySelectList"] = (await _nationalityServices.ListNationalitiesAsync())
                .Select(nationality => new SelectListItem
                {
                    Text = nationality.Description,
                    Value = nationality.Id.ToString()
                });
            ViewData["DepartmentSelectList"] = (await _departmentServices.ListDepartmentsAsync())
               .Select(department => new SelectListItem
               {
                   Text = department.Title,
                   Value = department.Id.ToString()
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

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}