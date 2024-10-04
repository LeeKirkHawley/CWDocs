using CWDocsCore.Extensions;
using CWDocsCore.Models;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
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
//using Castle.Core.Configuration;

namespace CWDocsCore.Services {
    public class DocumentService : IDocumentService {

        private readonly CWDocsDbContext _context;
        private readonly Logger _debugLogger;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _settings;


        public DocumentService(Microsoft.Extensions.Configuration.IConfiguration settings, CWDocsDbContext context) {
            _debugLogger = LogManager.GetLogger("debugLogger");
            _settings = settings;
            _context = context;
        }

        public List<DocumentModel> GetDocuments(UserModel user)
        {
            List<DocumentModel> docList = _context.Documents.Where(d => d.userId == user.Id).ToList();
            return docList;
        }

        public DocumentModel GetDocument(UserModel user, int id)
        {
            DocumentModel document = _context.Documents.Where(d => d.userId == user.Id && d.fileId == id).FirstOrDefault();
            return document;
        }

        public void DeleteDocument(int documentId) {
            DocumentModel document = _context.Documents.Where(d => d.fileId == documentId).FirstOrDefault();
            string documentFilePath = Path.Combine(_settings["DocumentFilePath"], document.documentName);

            var ret = _context.Remove(document);
            _context.SaveChanges();

            System.IO.File.Delete(documentFilePath);
        }
    }
}
