using CWDocsCore.Models;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;


namespace CWDocsCore.Services {
    public interface IDocumentService {
        public void DeleteDocument(int documentId);
    }
}
