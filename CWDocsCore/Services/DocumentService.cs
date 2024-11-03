using CWDocsCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;

namespace CWDocsCore.Services {
    public class DocumentService : IDocumentService {

        private readonly CWDocsDbContext _context;
        private readonly ILogger<DocumentService> _Logger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _settings;


        public DocumentService(Microsoft.Extensions.Configuration.IConfiguration settings, CWDocsDbContext context, ILogger<DocumentService> logger) {
//            _debugLogger = LogManager.GetLogger("debugLogger");
            _settings = settings;
            _context = context;
            _Logger = logger;
        }

        public List<DocumentModel> GetDocuments(UserModel user)
        {
            List<DocumentModel> docList = _context.Documents.Where(d => d.userId == user.Id).ToList();
            return docList;
        }

        public DocumentModel GetDocument(UserModel user, int id)
        {
            _Logger.LogWarning($"Getting document {id}");

            DocumentModel document = _context.Documents.Where(d => d.userId == user.Id && d.Id == id).FirstOrDefault();
            return document;
        }

        public DocumentModel CreateDocument(UserModel user, string originalFileName, string documentFilePath)
        {
            DocumentModel newDoc = _context.Documents.Add(new DocumentModel
            {
                userId = user.Id,
                documentName = Path.GetFileName(documentFilePath),
                originalDocumentName = originalFileName,
                documentDate = DateTime.Now.Ticks
            }).Entity;

            try
            {
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }

            return newDoc;
        }


        public void DeleteDocument(int documentId, string rootPath) {
            DocumentModel document = _context.Documents.Where(d => d.Id == documentId).FirstOrDefault();

            string documentFilePath = Path.Combine(rootPath, _settings["DownloadFilePath"], document.documentName);
            var ret = _context.Remove(document);
            _context.SaveChanges();

            if(File.Exists(documentFilePath))
            {
                System.IO.File.Delete(documentFilePath);
            }
            else
            {
                _Logger.LogWarning($"WARNING: trying to delete {documentFilePath}, but file doesn't exist.");
            }
        }
    }
}
