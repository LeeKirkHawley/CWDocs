using CWDocs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Services {
    public interface IDocumentService {
        public DataTablesModel LoadDocuments(User user, string draw, string start, string length, string sortColumn, string sortColumnDirection, string searchValue);
    }
}
