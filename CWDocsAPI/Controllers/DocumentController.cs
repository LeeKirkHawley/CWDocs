using CWDocsCore.Models;
using CWDocsCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CWDocsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DocumentController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;

        public static NLog.Logger _logger { get; set; } = LogManager.GetCurrentClassLogger();

        public DocumentController(/*ILogger<DocumentController> logger,*/ 
                                    IAccountService accountService,
                                    IDocumentService documentService,
                                    IUserService userService)
        {
            _accountService = accountService;
            _documentService = documentService;
            _userService = userService;
        }

        [HttpGet]
        [Route("Document")]
        //[DisableCors()]
        //public IActionResult Get(UserModel user)
        public IActionResult Get()
        {
            var httpUser = HttpContext.User.Identities.ToArray()[0];
            if (!httpUser.IsAuthenticated)
            {
                return RedirectToAction("login", "account");
            }


            UserModel user = _userService.GetAllowedUser(httpUser.Name);
            List<DocumentModel> docList = _documentService.GetDocuments(user);

            //Returning Json Data  
            var json = Json(new { data = docList });
            return json;
        }
    }
}
