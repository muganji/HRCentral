using HRCentral.Core.Data;
using HRCentral.Core.Models;
using HRCentral.Services.RelationshipTypes;
using HRCentral.Web.Models;
using HRCentral.Web.Models.RelationshipTypes;
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
    public class RelationshipController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IRelationshipTypeServices _relationshipTypeServices;
        private readonly ILogger<RelationshipController> _logger;
        /// <summary>
        /// The Constructor
        /// </summary>
        /// <param name="applicationDbContext"></param>
        /// <param name="relationshipTypeServices"></param>
        /// <param name="logger"></param>
        public RelationshipController(ApplicationDbContext applicationDbContext, IRelationshipTypeServices relationshipTypeServices, ILogger<RelationshipController> logger)
        {
            _db = applicationDbContext;
            _relationshipTypeServices = relationshipTypeServices;
            _logger = logger;
        }
        /// <summary>
        /// Returns a list of all relationships
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 10;

            var relationships = (await _relationshipTypeServices.ListRelationshipsAsync())
                .Select(relationship => new RelationshipTypeListViewModel
                {
                    Title = relationship.Title,
                    Id = relationship.Id,
                    DateAdded = relationship.DateTimeAdded == null ? string.Empty : DateTime.Parse(relationship.DateTimeAdded.ToString()).ToString("yyyy-MM-dd"),
                    DateModified = relationship.DateTimeModified == null ? string.Empty : DateTime.Parse(relationship.DateTimeModified.ToString()).ToString("yyyy-MM-dd"),
                    CreatedBy = relationship.UserAccount
                }).ToPagedList(pageSize, pageNumber);

            return View(relationships);
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var relationshipQuery = await _relationshipTypeServices.GetRelationshipsById(id);
            if (relationshipQuery == null)
            {
                return NotFound();
            }
            await _relationshipTypeServices.DeleteRelationshipAsync(relationshipQuery); ;
            TempData["Message"] = "Record deleted successfully";
            _logger.LogInformation($"Success: successfully deleted relationship record by user={@User.Identity.Name.Substring(4)}");
            return RedirectToAction("index");
        }
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Deletors")]
        public IActionResult Delete()
        {
            return View();
        }
        /// <summary>
        /// Initializes the update method.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(Guid id)
        {
            var relationshipQuery = await _relationshipTypeServices.GetRelationshipsById(id);

            if (relationshipQuery == null)
            {
                return NotFound();
            }

            var model = new RelationshipTypeDetailViewModel
            {
                Title = relationshipQuery.Title,
                Id = relationshipQuery.Id,
            };

            return View(model);
        }
        /// <summary>
        /// Updates existing relationships in the database
        /// </summary>
        /// <param name="formData"></param>
        /// <returns></returns>
       [Authorize(Roles = "ACL-Developers,ACL-HRCentralDatabase-Admins")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Details(RelationshipTypeDetailViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _relationshipTypeServices.UpdateRelationshipTypeAsync(new RelationShipType
                    {
                        DateTimeModified = DateTimeOffset.Now,
                        Title = formData.Title,
                        Id = formData.Id,
                        UserAccount = User.Identity.Name
                    });
                    TempData["Message"] = "Changes saved successfully";
                    _logger.LogInformation($"Success: successfully updated {formData.Title} Relationship record by user={@User.Identity.Name.Substring(4)}");
                    return RedirectToAction("details", new { id = formData.Id });
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Relationship", $"Failed to update record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to update {formData.Title} Relationship. Internal Application Error.; user={@User.Identity.Name.Substring(4)}");
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
        public async Task<IActionResult> Add(NewRelationshipViewModel formData)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    bool bIfExist = false;
                    var q = from c in _db.RelationShipTypes where c.Title == formData.Title select c;
                    try
                    {
                        q.ToList()[0].Title.ToString();
                        bIfExist = true;
                    }
                    catch { }
                    if (bIfExist == true)
                    {
                        ModelState.AddModelError("Relationship", $"Can not register duplicate record. {formData.Title} Relationship is already registered");
                    }
                    else
                    {
                        await _relationshipTypeServices.AddRelationshiptypeAsync(new RelationShipType
                        {
                            DateTimeAdded = DateTimeOffset.Now,
                            Title = formData.Title,
                            DateTimeModified = DateTimeOffset.Now,
                            UserAccount = User.Identity.Name,
                        });
                        TempData["Message"] = "Relationship Successfully Added";
                        _logger.LogInformation($"Success: successfully added {formData.Title} relationship record by user={@User.Identity.Name.Substring(4)}");
                        return RedirectToAction("add");
                    }
                }
            }
            catch (ApplicationException error)
            {
                ModelState.AddModelError("Relationship", $"Failed to register record. {formData.Title} Contact IT ServiceDesk for support.");
                _logger.LogError(
                    error,
                    $"FAIL: failed to register {formData.Title} Relationship. Internal Application Error; user={@User.Identity.Name.Substring(4)}");
            }
            return View(formData);
        }
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}