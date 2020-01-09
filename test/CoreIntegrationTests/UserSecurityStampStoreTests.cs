namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    public class UserSecurityStampStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task Create_NewUser_HasSecurityStamp()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };

            await manager.CreateAsync(user);

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.NotNull(savedUser.SecurityStamp);
        }

        [Fact]
        public async Task GetSecurityStamp_NewUser_ReturnsStamp()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            var stamp = await manager.GetSecurityStampAsync(user);

            Assert.NotNull(stamp);
        }
    }
}