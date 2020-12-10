using CWDocs.Extensions;
using CWDocs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Services {
    public class DocumentService : IDocumentService {

        private readonly CWDocsDbContext _context;

        public DocumentService(CWDocsDbContext context) {
            _context = context;
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
    }
}
