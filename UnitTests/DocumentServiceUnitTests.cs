using CWDocsCore.Services;
using CWDocsCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Moq;
using Xunit;
using CWDocsCore.Models;

namespace CWDocsCore_UnitTests
{
    public class DocumentServiceUnitTests
    {
        private readonly Mock<IDocumentService> _userService = new Mock<IDocumentService>();
        private readonly IConfiguration _configuration;
        CWDocsDbContext _dBcontext;

        public DocumentServiceUnitTests()
        {
            _configuration = CreateConfiguration();
            _dBcontext = CreateDbContext();
        }


        [Fact]
        public void ShouldGetDocuments()
        {
            UserModel userModel = new UserModel {Id = 1, userName = "Kirk", pwd = "pwd", role = "user" };

            List<DocumentModel> documentModels = GetSut().GetDocuments(userModel);

            Assert.NotNull(documentModels);
            Assert.NotEmpty(documentModels);
        }

        [Fact]
        public void ShouldCreateDocument()
        {
            UserModel userModel = new UserModel { Id = 1, userName = "Kirk", pwd = "pwd", role = "user" };

            DocumentModel documentModel = GetSut().CreateDocument(userModel, "someFileName", "someFilePath");

            Assert.NotNull(documentModel);
        }

        [Fact]
        public void ShouldGetDocument()
        {
            UserModel userModel = new UserModel { Id = 1, userName = "Kirk", pwd = "pwd", role = "user" };

            DocumentModel documentModel = GetSut().GetDocument(userModel, 1);

            Assert.NotNull(documentModel);
        }


        private IConfiguration CreateConfiguration()
        {
            // https://medium.com/@TheLe0/mocking-your-appsettings-in-unit-tests-on-net-cb057de7db64
            var inMemorySettings = new Dictionary<string, string> {
                {"SQLiteDataContext", "CWDocs"},
                {"SQLiteDbPath", "C:\\Work\\A_My_Websites\\CWDocs\\UnitTests\\CWDocs.db"},
                //{"SectionName:SomeKey", "SectionValue"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            return configuration;
        }

        private CWDocsDbContext CreateDbContext()
        {
            return new CWDocsDbContext(_configuration);
        }

        DocumentService GetSut()
        {
            return new DocumentService(_configuration, _dBcontext);
        }

    }
}
