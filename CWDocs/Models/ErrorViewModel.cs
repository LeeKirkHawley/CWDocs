using System;

namespace CWDocs.Models {
    public class ErrorViewModel {
        public string RequestId { get; set; }
        public string ExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string CustomDetails { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
