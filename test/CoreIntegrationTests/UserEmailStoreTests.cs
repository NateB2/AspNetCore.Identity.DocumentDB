namespace IntegrationTests
{
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;
    
    public class UserEmailStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task Create_NewUser_HasNoEmail()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            var email = await manager.GetEmailAsync(user);

            Assert.Null(email);
        }

        [Fact]
        public async Task SetEmail_SetsEmail()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            await manager.SetEmailAsync(user, "email");

            Assert.Equal("email", await manager.GetEmailAsync(user));
        }

        [Fact]
        public async Task FindUserByEmail_ReturnsUser()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            Assert.Null(await manager.FindByEmailAsync("email"));

            await manager.SetEmailAsync(user, "email");

            Assert.NotNull(await manager.FindByEmailAsync("email"));
        }

        [Fact]
        public async Task Create_NewUser_IsNotEmailConfirmed()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            var isConfirmed = await manager.IsEmailConfirmedAsync(user);

            Assert.False(isConfirmed);
        }

        [Fact]
        public async Task SetEmailConfirmed_IsConfirmed()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            var token = await manager.GenerateEmailConfirmationTokenAsync(user);

            await manager.ConfirmEmailAsync(user, token);

            var isConfirmed = await manager.IsEmailConfirmedAsync(user);
            Assert.True(isConfirmed);
        }

        [Fact]
        public async Task ChangeEmail_AfterConfirmedOriginalEmail_NotEmailConfirmed()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            var token = await manager.GenerateEmailConfirmationTokenAsync(user);
            await manager.ConfirmEmailAsync(user, token);

            await manager.SetEmailAsync(user, "new@email.com");

            var isConfirmed = await manager.IsEmailConfirmedAsync(user);
            Assert.False(isConfirmed);
        }
    }
}