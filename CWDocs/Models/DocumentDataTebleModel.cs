using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Models {
    public class DocumentDataTableModel {
        public int fileId { get; set; }
        public int userId { get; set; }
        public string originalDocumentName { get; set; }
        public string documentName { get; set; }
        public string documentDate { get; set; } // will need to convert this from the long value in db
    }
}
