namespace IntegrationTests
{
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    // todo low - validate all tests work
    public class UserTwoFactorStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task SetTwoFactorEnabled()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            await manager.SetTwoFactorEnabledAsync(user, true);

            Assert.True(await manager.GetTwoFactorEnabledAsync(user));
        }

        [Fact]
        public async Task ClearTwoFactorEnabled_PreviouslyEnabled_NotEnabled()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            await manager.SetTwoFactorEnabledAsync(user, true);

            await manager.SetTwoFactorEnabledAsync(user, false);

            Assert.False(await manager.GetTwoFactorEnabledAsync(user));
        }
    }
}