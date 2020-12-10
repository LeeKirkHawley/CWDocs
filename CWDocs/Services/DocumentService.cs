using CWDocs.Extensions;
using CWDocs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NLog;
using System.Threading;
using System.IO;
using Castle.Core.Configuration;

namespace CWDocs.Services {
    public class DocumentService : IDocumentService {

        private readonly CWDocsDbContext _context;
        private readonly Logger _debugLogger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _settings;


        public DocumentService(Microsoft.Extensions.Configuration.IConfiguration settings, CWDocsDbContext context) {
            _debugLogger = LogManager.GetLogger("debugLogger");
            _context = context;
            _settings = settings;
        }

        public DataTablesModel LoadDocuments(User user, string draw, string start, string length, string sortColumn, string sortColumnDirection, string searchValue) {
            //Paging Size (10,20,50,100)  
            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int recordsTotal = 0;

            // get documents from db
            List<Document> docList = _context.Documents.Where(d => d.userId == user.Id).ToList();
            List<DocumentDataTableModel> docDataTableModel = new List<DocumentDataTableModel>();
            foreach (Document doc in docList) {
                DocumentDataTableModel m = new DocumentDataTableModel();

                m.fileId = doc.fileId;
                m.userId = doc.userId;
                m.originalDocumentName = doc.originalDocumentName;
                m.documentName = doc.documentName;
                m.documentDate = new DateTime(doc.documentDate).ToString();

                docDataTableModel.Add(m);
            }

            //Sorting  
            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection))) {
                docList = OrderByExtension.OrderBy<Document>(docList.AsQueryable<Document>(), sortColumn).ToList();
            }

            //Search  
            if (!string.IsNullOrEmpty(searchValue)) {
                docList = docList.Where(m => m.originalDocumentName.Contains(searchValue)).ToList();
            }

            //total number of rows count   
            recordsTotal = docList.Count();

            List<DocumentDataTableModel> data = docDataTableModel.Skip(skip).Take(pageSize).ToList();

            DataTablesModel dtModel = new DataTablesModel {
                draw = draw,
                recordsFiltered = data.Count,
                recordsTotal = recordsTotal,
                data = data,
            };

            return dtModel;
        }

        public void UploadDocuments(CWDocsUploadDocsViewModel model, IFormFile[] files, ClaimsPrincipal User) {
            ClaimsIdentity identity = User.Identities.ToArray()[0];
            if (!identity.IsAuthenticated) {
                throw new Exception("user is not logged in.");
            }

            User user = _context.Users.Where(u => u.userName == User.Identities.ToArray()[0].Name).FirstOrDefault();

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
        }

    }
}
