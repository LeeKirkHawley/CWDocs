using CWDocsCore.Models;
using CWDocsCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        //private readonly ILogger<DocumentController> _logger;
        private readonly IAccountService _accountService;
        private readonly IDocumentService _documentService;
        private readonly IUserService _userService;

        public DocumentController(/*ILogger<DocumentController> logger,*/ 
                                    IAccountService accountService,
                                    IDocumentService documentService,
                                    IUserService userService)
        {
            //_logger = logger;
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

            //return Ok("Help me!");


            //Returning Json Data  
            var json = Json(new { data = docList });
            return json;
        }
    }
}
