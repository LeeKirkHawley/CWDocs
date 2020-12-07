using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Models {
    public class CWDocsViewModel {
        public int documentId { get; set; }
        public string image { get; set; }
        public string documentName { get; set; }
        public string originalDocumentName { get; set; }
        public string documentDate { get; set; }
    }
}
