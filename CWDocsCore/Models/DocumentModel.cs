using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocsCore.Models {
    public class DocumentModel {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        public int userId { get; set; }
        public string originalDocumentName { get; set; }
        public string documentName { get; set; }
        public long documentDate { get; set; }
    }
}
