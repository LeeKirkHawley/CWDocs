using CWDocs.Extensions;
using CWDocs.Models;
using CWDocs.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CWDocs.Controllers {
    public class HomeController : Controller {
        private readonly Logger _debugLogger;
        private readonly IConfiguration _settings;
        //private readonly IOCRService _ocrService;
        private readonly IWebHostEnvironment _environment;
        private readonly CWDocsDbContext _context;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly AccountController _accountController;
        private readonly IDocumentService _documentService;


        public HomeController(IConfiguration settings,
                                //IOCRService ocrService,
                                IWebHostEnvironment environment,
                                CWDocsDbContext context,
                                IUserService userService,
                                IAccountService accountService,
                                IDocumentService documentService,
                                AccountController accountController) {
            _debugLogger = LogManager.GetLogger("debugLogger"); 
            _settings = settings;
//          _ocrService = ocrService;
            _environment = environment;
            _context = context;
            _userService = userService;
            _accountService = accountService;
            _documentService = documentService;
            _accountController = accountController;

//            _ocrService.SetupLanguages();

        }

        public IActionResult Index() {
            var user = HttpContext.User.Identities.ToArray()[0];
            if (!user.IsAuthenticated) {
                return RedirectToAction("login", "account");
            }

            CWDocsIndexViewModel model = new CWDocsIndexViewModel();

            _accountController.Login();

            return View(model);
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult UploadDoc() {

            var user = HttpContext.User.Identities.ToArray()[0];
            if (!user.IsAuthenticated) {
                return RedirectToAction("login", "account");
            }

            CWDocsUploadDocsViewModel model = new CWDocsUploadDocsViewModel();

            return View(model);
        }

        [HttpPost]
        public ActionResult UploadDoc(CWDocsUploadDocsViewModel model, IFormFile[] files) {
            _documentService.UploadDocuments(model, files, HttpContext.User);

            // redirect to home page
            return Redirect("/");
        }


        [HttpPost]
        public IActionResult LoadDocs() {
            User user = _context.Users.Where(u => u.userName == HttpContext.User.Identities.ToArray()[0].Name).FirstOrDefault();
            // if user is not logged in, don't show nothin
            if (user == null) {
                List<Document> emptyList = new List<Document>();
                var errorJson = Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = emptyList });
                return errorJson;
            }

            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();

            // Skip number of Rows count  
            var start = Request.Form["start"].FirstOrDefault();

            // Paging Length 10,20  
            var length = Request.Form["length"].FirstOrDefault();

            // Sort Column Name  
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();

            // Sort Column Direction (asc, desc)  
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            // Search Value from (Search box)  
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            DataTablesModel dtModel = _documentService.LoadDocuments(user, draw, start, length, sortColumn, sortColumnDirection, searchValue);

            var json = Json(new { draw = draw, 
                                  recordsFiltered = dtModel.recordsFiltered, 
                                  recordsTotal = dtModel.recordsTotal, 
                                  data = dtModel.data });

            return json;
        }

        [HttpGet]
        public IActionResult View(int Id) {

            Document document = _context.Documents.Where(d => d.fileId == Id).FirstOrDefault();
            string documentFilePath = Path.Combine(_settings["HTMLFilePath"], document.documentName);

            CWDocsViewModel model = new CWDocsViewModel {
                documentId = document.fileId,
                image = documentFilePath,
                documentName = document.documentName,
                originalDocumentName = document.originalDocumentName,
                documentDate = new DateTime(document.documentDate).ToString()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete([FromForm] int documentId) {
            _documentService.DeleteDocument(documentId);
            return Ok();
        }
    }
}
