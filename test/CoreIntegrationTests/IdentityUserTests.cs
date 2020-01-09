namespace IntegrationTests
{
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    public class IdentityUserTests : UserIntegrationTestsBase
    {
        [Fact]
        public void Insert_NoId_SetsId()
        {
            var user = new DocDBIdentity.IdentityUser();
            user.Id = null;

            user = (dynamic)Client.CreateDocumentAsync(Users.DocumentsLink, user).Result.Resource;

            Assert.NotNull(user);
            Assert.NotNull(user.Id);
        }
    }
}