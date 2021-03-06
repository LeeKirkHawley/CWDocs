﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocs.Models {
    public class CWDocsIndexViewModel {
        public string OCRText { get; set; }
        public string OriginalFileName { get; set; }

        public string CacheFilename { get; set; }

        [Required]
        [Display(Name = "Language")]
        public string Language { get; set; }

        [Required]
        [Display(Name = "Avatar")]
        public string Avatar { get; set; }

        public List<SelectListItem> Languages { get; set; }

    }
}
