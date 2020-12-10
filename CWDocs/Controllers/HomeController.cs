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

            ClaimsIdentity identity = HttpContext.User.Identities.ToArray()[0];
            if (!identity.IsAuthenticated) {
                throw new Exception("user is not logged in.");
            }

            User user = _context.Users.Where(u => u.userName == HttpContext.User.Identities.ToArray()[0].Name).FirstOrDefault();

            DateTime startTime = DateTime.Now;

            string file = "";
            try {
                file = $"{files[0].FileName}";
            }
            catch (Exception ex) {
                _debugLogger.Debug(ex, "Exception reading file name.");
            }

            _debugLogger.Info($"Thread {Thread.CurrentThread.ManagedThreadId}: Processing file {file}");


            // Extract file name from whatever was posted by browser
            var originalFileName = System.IO.Path.GetFileName(files[0].FileName);
            string imageFileExtension = Path.GetExtension(originalFileName);

            var fileName = Guid.NewGuid().ToString();

            // set up the document file (input) path
            string documentFilePath = Path.Combine(_settings["DocumentFilePath"], fileName);
            documentFilePath += imageFileExtension;

            //_debugLogger.Info($"ImageFilePath: {imageFilePath}");
            //_debugLogger.Info($"Current: {Directory.GetCurrentDirectory()}");

            // set up the text file (output) path
            //string textFilePath = Path.Combine(_settings["TextFilePath"], fileName);

            // If file with same name exists
            if (System.IO.File.Exists(documentFilePath)) {
                throw new Exception($"Document {documentFilePath} already exists!");
            }

            // Create new local file and copy contents of uploaded file
            try {
                using (var localFile = System.IO.File.OpenWrite(documentFilePath))
                using (var uploadedFile = files[0].OpenReadStream()) {
                    uploadedFile.CopyTo(localFile);

                    // update model for display of ocr'ed data
                    model.OriginalFileName = originalFileName;

                    DateTime finishTime = DateTime.Now;
                    TimeSpan ts = (finishTime - startTime);
                    string duration = ts.ToString(@"hh\:mm\:ss");

                    _debugLogger.Info($"Thread {Thread.CurrentThread.ManagedThreadId}: Finished uploading document {file} to {localFile} Elapsed time: {duration}");
                }
            }
            catch (Exception ex) {
                _debugLogger.Debug($"Couldn't write file {documentFilePath}");
                // HANDLE ERROR
                throw;
            }

            //string errorMsg = "";

            //if (imageFileExtension.ToLower() == ".pdf") {
            //    await _ocrService.OCRPDFFile(imageFilePath, textFilePath + ".tif", "eng");

            //}
            //else {
            //    errorMsg = await _ocrService.OCRImageFile(imageFilePath, textFilePath, "eng");
            //}

            //string textFileName = textFilePath + ".txt";
            //string ocrText = "";
            //try {
            //    ocrText = System.IO.File.ReadAllText(textFileName);
            //}
            //catch (Exception ex) {
            //    _debugLogger.Debug($"Couldn't read text file {textFileName}");
            //}

            //if (ocrText == "") {
            //    if (errorMsg == "")
            //        ocrText = "No text found.";
            //    else
            //        ocrText = errorMsg;
            //}


            Document newDoc = _context.Documents.Add(new Document {
                userId = user.Id,
                documentName = Path.GetFileName(documentFilePath),
                originalDocumentName = originalFileName, 
                documentDate = DateTime.Now.Ticks
            }).Entity;

            try {
                _context.SaveChanges();
            }
            catch (Exception e) {
                throw;
            }

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

            Document document = _context.Documents.Where(d => d.fileId == documentId).FirstOrDefault();
            string documentFilePath = Path.Combine(_settings["DocumentFilePath"], document.documentName);

            var ret = _context.Remove(document);
            _context.SaveChanges();

            System.IO.File.Delete(documentFilePath);

            return Ok();
        }
    }
}
