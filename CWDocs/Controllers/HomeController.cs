﻿using CWDocsCore.Extensions;
using CWDocs.Models;
using CWDocsCore.Services;
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
using CWDocsCore;
using CWDocsCore.Models;


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

            DataTablesModel dtModel = LoadDocuments(user, draw, start, length, sortColumn, sortColumnDirection, searchValue);

            var json = Json(new { draw = draw, 
                                  recordsFiltered = dtModel.recordsFiltered, 
                                  recordsTotal = dtModel.recordsTotal, 
                                  data = dtModel.data });

            return json;
        }

        public DataTablesModel LoadDocuments(UserModel user, string draw, string start, string length, string sortColumn, string sortColumnDirection, string searchValue) {
            //Paging Size (10,20,50,100)  
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;
            int recordsFiltered = 0;

            // get documents from db
            List<DocumentModel> docList = _context.Documents.Where(d => d.userId == user.Id).ToList();

            recordsTotal = docList.Count();
            recordsFiltered = docList.Count();

            //Sorting  
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection))) {
                docList = OrderByExtension.OrderBy<DocumentModel>(docList.AsQueryable<DocumentModel>(), sortColumn).ToList();
            }

            //Search  
            if (!string.IsNullOrEmpty(searchValue)) {
                docList = docList.Where(m => m.originalDocumentName.Contains(searchValue)).ToList();
                recordsFiltered = docList.Count();
            }

            //total number of rows count   

            List<DocumentDataTableModel> docDataTableModel = new List<DocumentDataTableModel>();
            foreach (DocumentModel doc in docList) {
                DocumentDataTableModel m = new DocumentDataTableModel();

                m.fileId = doc.fileId;
                m.userId = doc.userId;
                m.originalDocumentName = doc.originalDocumentName;
                m.documentName = doc.documentName;
                m.documentDate = new DateTime(doc.documentDate).ToString();

                docDataTableModel.Add(m);
            }

            List<DocumentDataTableModel> data = docDataTableModel.Skip(skip).Take(pageSize).ToList();

            DataTablesModel dtModel = new DataTablesModel {
                draw = draw,
                recordsFiltered = recordsFiltered, // records after search - NOT the page count
                recordsTotal = recordsTotal,       // total records before any searching or pagination or anything
                data = data,
            };

            return dtModel;
        }


        [HttpGet]
        public IActionResult View(int Id) {

            DocumentModel document = _context.Documents.Where(d => d.fileId == Id).FirstOrDefault();
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

        public void UploadDocuments(CWDocsUploadDocsViewModel model, IFormFile[] files, ClaimsPrincipal User) {
            ClaimsIdentity identity = User.Identities.ToArray()[0];
            if (!identity.IsAuthenticated) {
                throw new Exception("user is not logged in.");
            }

            UserModel user = _context.Users.Where(u => u.userName == User.Identities.ToArray()[0].Name).FirstOrDefault();

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


            DocumentModel newDoc = _context.Documents.Add(new DocumentModel {
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
        }


    }
}
