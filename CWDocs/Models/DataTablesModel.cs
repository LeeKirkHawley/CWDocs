using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Models {
    public class DataTablesModel {
        public string draw { get; set; }
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public List<DocumentDataTableModel> data { get; set; }
    }
}
