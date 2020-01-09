namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    // todo low - validate all tests work
    public class UserPasswordStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task HasPassword_NoPassword_ReturnsFalse()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            var hasPassword = await manager.HasPasswordAsync(user);

            Assert.False(hasPassword);
        }

        [Fact]
        public async Task AddPassword_NewPassword_CanFindUserByPassword()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = CreateServiceProvider<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                 {
                     options.Password.RequireDigit = false;
                     options.Password.RequireNonAlphanumeric = false;
                     options.Password.RequireUppercase = false;
                 })
                .GetService<UserManager<DocDBIdentity.IdentityUser>>();
            await manager.CreateAsync(user);

            var result = await manager.AddPasswordAsync(user, "testtest");
            Assert.True(result.Succeeded);

            var userByName = await manager.FindByNameAsync("bob");
            Assert.NotNull(userByName);
            var passwordIsValid = await manager.CheckPasswordAsync(userByName, "testtest");
            Assert.True(passwordIsValid);
        }

        [Fact]
        public async Task RemovePassword_UserWithPassword_SetsPasswordNull()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            await manager.AddPasswordAsync(user, "testtest");

            await manager.RemovePasswordAsync(user);

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.Null(savedUser.PasswordHash);
        }
    }
}