using CWDocsCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CWDocsCore.Services {
    public interface IDocumentService {
        List<DocumentModel> GetDocuments(UserModel user);
        DocumentModel CreateDocument(UserModel user, string originalFileName, string documentFilePath);
        void DeleteDocument(int documentId, string rootPath);
    }
}
