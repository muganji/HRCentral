using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRCentral.Services.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HRCentral.Core.Data;
// Excel Libraries
using System.IO;
using System.Data;
using System.Web;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;

namespace HRCentral.Web.Controllers
{
    [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
    public class ExcelController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmployeeServices _employeeServices;
        public ExcelController(ApplicationDbContext applicationDbContext, IEmployeeServices employeeServices)
        {
            _db = applicationDbContext;
            _employeeServices = employeeServices;
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public IActionResult Index()
        {

            return View();
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        public FileResult Export()
        {
            DataTable dt = new DataTable("Employee");
            var employees = from employee in _db.Employees.Where(emp => emp.IsActive) select employee;
            dt.Columns.AddRange(new DataColumn[11] {
                new DataColumn("Date Of Join"),
                new DataColumn("PFNumber"),
             new DataColumn("Full Name"),
            new DataColumn("BirthDate"),
            new DataColumn("Gender"),
            new DataColumn("Residence"),
                new DataColumn("Phone Number"),
                new DataColumn("Office Number"),
                new DataColumn("Job Title"),
                new DataColumn("Manager Name"),
                new DataColumn("Status")
               

            });
            foreach (var employee in employees)
            {
                dt.Rows.Add(
                    DateTime.Parse(employee.JoinDate.ToString()).ToString("yyyy-MM-dd"),
                    employee.PersonnelFileNumber,
                    employee.FullName,
                    DateTime.Parse(employee.DateOfBirth.ToString()).ToString("yyyy-MM-dd"),
                    employee.Gender,
                    employee.Address,
                    employee.PersonalMobileNumber,
                    employee.WorkMobileNumber,
                    employee.JobTitle,
                    employee.ManagerName,
                    employee.IsActive
                   

                    );
            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ServingEmployees.xlsx");
                }
            }
        }
       

    }
}