using CWDocsCore.Extensions;
using CWDocs.Models;
using CWDocsCore.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using CWDocsCore;
using CWDocsCore.Models;
using Microsoft.AspNetCore.Diagnostics;


namespace CWDocs.Controllers {
    public class HomeController : Controller {
        private readonly IConfiguration _settings;
        //private readonly IOCRService _ocrService;
        private readonly IWebHostEnvironment _environment;
        private readonly CWDocsDbContext _context;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly AccountController _accountController;
        private readonly IDocumentService _documentService;

        public static NLog.Logger _logger { get; set; } = LogManager.GetCurrentClassLogger();


        public HomeController(IConfiguration settings,
                                //IOCRService ocrService,
                                IWebHostEnvironment environment,
                                CWDocsDbContext context,
                                IUserService userService,
                                IAccountService accountService,
                                IDocumentService documentService,
                                AccountController accountController) {
            _settings = settings;
//          _ocrService = ocrService;
            _environment = environment;
            _context = context;
            _userService = userService;
            _accountService = accountService;
            _documentService = documentService;
            _accountController = accountController;

            //            _ocrService.SetupLanguages();

        _logger.Debug("TEST LOGGING");

        }

        public IActionResult Index() {
            var user = HttpContext.User.Identities.ToArray()[0];
            if (!user.IsAuthenticated) {
                return RedirectToAction("login", "account");
            }

            CWDocsIndexViewModel model = new CWDocsIndexViewModel();

            //var l =_accountController.Login();

            return View(model);
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            //return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = exceptionHandlerPathFeature?.Error;

            // Create a view model or dictionary to hold error details
            var errorModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ExceptionMessage = exception?.Message,
                StackTrace = exception?.StackTrace,
                // Add any other details you want to show or log
                CustomDetails = "This could be any additional context or data you want to display."
            };

            // Here you can log the exception if needed
            //_logger.LogError(exception, "An unhandled exception has occurred.");

            return View(errorModel);
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
            UploadDocuments(model, files, HttpContext.User);

            // redirect to home page
            return Redirect("/");
        }


        [HttpPost]
        public IActionResult LoadDocs() {
            UserModel user = _context.Users.Where(u => u.userName == HttpContext.User.Identities.ToArray()[0].Name).FirstOrDefault();
            // if user is not logged in, don't show nothin
            if (user == null) {
                List<DocumentModel> emptyList = new List<DocumentModel>();
                var errorJson = Json(new { draw = 0, recordsFiltered = 0, recordsTotal = 0, data = emptyList });
                return errorJson;
            }

            List<DocumentModel> docList = _documentService.GetDocuments(user);

            // most of the following code should be there for paging etc. on the DataTable
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

            DataTablesModel dtModel = FilterDocs(docList, draw, start, length, sortColumn, sortColumnDirection, searchValue);

            var json = Json(new { draw = draw, 
                                  recordsFiltered = dtModel.recordsFiltered, 
                                  recordsTotal = dtModel.recordsTotal, 
                                  data = dtModel.data });

            return json;
        }

        public DataTablesModel FilterDocs(List<DocumentModel> docList, string draw, string start, string length, string sortColumn, string sortColumnDirection, string searchValue)
        {
            int pageSize, skip, recordsTotal, recordsFiltered;
            recordsTotal = 0;
            recordsFiltered = 0;
            //Paging Size (10,20,50,100)  
            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;

            recordsTotal = docList.Count();
            recordsFiltered = docList.Count();

            //Sorting  
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            {
                docList = OrderByExtension.OrderBy<DocumentModel>(docList.AsQueryable<DocumentModel>(), sortColumn).ToList();
            }

            //Search  
            if (!string.IsNullOrEmpty(searchValue))
            {
                docList = docList.Where(m => m.originalDocumentName.Contains(searchValue)).ToList();
                recordsFiltered = docList.Count();
            }

            //total number of rows count   

            List<DocumentDataTableModel> docDataTableModel = new List<DocumentDataTableModel>();
            foreach (DocumentModel doc in docList)
            {
                DocumentDataTableModel m = new DocumentDataTableModel();

                m.fileId = doc.Id;
                m.userId = doc.userId;
                m.originalDocumentName = doc.originalDocumentName;
                m.documentName = doc.documentName;
                m.documentDate = new DateTime(doc.documentDate).ToString();

                docDataTableModel.Add(m);
            }

            List<DocumentDataTableModel> data = docDataTableModel.Skip(skip).Take(pageSize).ToList();

            DataTablesModel dtModel = new DataTablesModel
            {
                draw = draw,
                recordsFiltered = recordsFiltered, // records after search - NOT the page count
                recordsTotal = recordsTotal,       // total records before any searching or pagination or anything
                data = data,
            };

            return dtModel;
        }

        [HttpGet]
        public IActionResult View(int Id) {

            DocumentModel document = _context.Documents.Where(d => d.Id == Id).FirstOrDefault();

            string documentFilePath = Path.Combine(_settings["UploadFilePath"], document.documentName);

            CWDocsViewModel model = new CWDocsViewModel {
                documentId = document.Id,
                image = documentFilePath,
                documentName = document.documentName,
                originalDocumentName = document.originalDocumentName,
                documentDate = new DateTime(document.documentDate).ToString()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Delete([FromForm] int documentId) 
        {
            _documentService.DeleteDocument(documentId, _environment.WebRootPath);
            return Ok();
        }

        public void UploadDocuments(CWDocsUploadDocsViewModel model, IFormFile[] files, ClaimsPrincipal User)
        {
            ClaimsIdentity identity = User.Identities.ToArray()[0];
            if (!identity.IsAuthenticated)
            {
                throw new Exception("user is not logged in.");
            }

            UserModel user = _context.Users.Where(u => u.userName == User.Identities.ToArray()[0].Name).FirstOrDefault();

            DateTime startTime = DateTime.Now;

            string file = "";
            try
            {
                file = $"{files[0].FileName}";
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Exception reading file name.");
            }

            _logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId}: Processing file {file}");


            // Extract file name from whatever was posted by browser
            var originalFileName = System.IO.Path.GetFileName(files[0].FileName);
            string imageFileExtension = Path.GetExtension(originalFileName);

            var fileName = Guid.NewGuid().ToString();

            // set up the document file (input) path
            var webRootPath = _environment.WebRootPath;
            string documentFilePath = Path.Combine(webRootPath, _settings["DownloadFilePath"], fileName);
            documentFilePath += imageFileExtension;

            // If file with same name exists
            if (System.IO.File.Exists(documentFilePath))
            {
                throw new Exception($"Document {documentFilePath} already exists!");
            }

            // Create new local file and copy contents of uploaded file
            try
            {
                using (var localFile = System.IO.File.OpenWrite(documentFilePath))
                using (var uploadedFile = files[0].OpenReadStream())
                {
                    uploadedFile.CopyTo(localFile);
                    bool fileExists = System.IO.File.Exists(documentFilePath);

                    // update model for display of ocr'ed data
                    model.OriginalFileName = originalFileName;

                    DateTime finishTime = DateTime.Now;
                    TimeSpan ts = (finishTime - startTime);
                    string duration = ts.ToString(@"hh\:mm\:ss");

                    _logger.Info($"Thread {Thread.CurrentThread.ManagedThreadId}: Finished uploading document {file} to {localFile} Elapsed time: {duration}");
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Couldn't write file {documentFilePath}");
                // HANDLE ERROR
                throw;
            }

            var doesFileExist = System.IO.File.Exists(documentFilePath);
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


            _documentService.CreateDocument(user, originalFileName, documentFilePath);
        }

    }
}
