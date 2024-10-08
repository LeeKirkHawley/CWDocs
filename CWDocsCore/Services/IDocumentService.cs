﻿using CWDocsCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CWDocsCore.Services {
    public interface IDocumentService {
        public List<DocumentModel> GetDocuments(UserModel user);
        public void DeleteDocument(int documentId);
    }
}
