namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    // todo low - validate all tests work
    public class UserLoginStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task AddLogin_NewLogin_Adds()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AddLoginAsync(user, login);

            var savedLogin = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault().Logins.Single();
            Assert.Equal("provider", savedLogin.LoginProvider);
            Assert.Equal("key", savedLogin.ProviderKey);
            Assert.Equal("name", savedLogin.ProviderDisplayName);
        }

        [Fact]
        public async Task RemoveLogin_NewLogin_Removes()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddLoginAsync(user, login);

            await manager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.Empty(savedUser.Logins);
        }

        [Fact]
        public async Task GetLogins_OneLogin_ReturnsLogin()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddLoginAsync(user, login);

            var logins = await manager.GetLoginsAsync(user);

            var savedLogin = logins.Single();
            Assert.Equal("provider", savedLogin.LoginProvider);
            Assert.Equal("key", savedLogin.ProviderKey);
            Assert.Equal("name", savedLogin.ProviderDisplayName);
        }

        [Fact]
        public async Task Find_UserWithLogin_FindsUser()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddLoginAsync(user, login);

            var findUser = await manager.FindByLoginAsync(login.LoginProvider, login.ProviderKey);

            Assert.NotNull(findUser);
        }

        [Fact]
        public async Task Find_UserWithDifferentKey_DoesNotFindUser()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddLoginAsync(user, login);

            var findUser = await manager.FindByLoginAsync("provider", "otherkey");

            Assert.Null(findUser);
        }

        [Fact]
        public async Task Find_UserWithDifferentProvider_DoesNotFindUser()
        {
            var manager = GetUserManager();
            var login = new UserLoginInfo("provider", "key", "name");
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddLoginAsync(user, login);

            var findUser = await manager.FindByLoginAsync("otherprovider", "key");

            Assert.Null(findUser);
        }
    }
}