namespace IntegrationTests
{
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;
    using System.Security.Claims;

    // todo low - validate all tests work
    public class UserRoleClaimStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task GetClaims_UserHasNoRoles_UserHasNoClaims_ReturnsNoClaims()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            var claims = await manager.GetClaimsAsync(user);

            Assert.Empty(claims);
        }

        [Fact]
        public async Task GetClaims_UserHasRoles_UserHasNoRoleClaims_ReturnsNoClaims()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AddToRoleAsync(user, "role");

            var claims = await manager.GetClaimsAsync(user);

            Assert.Empty(claims);
        }
    }
}