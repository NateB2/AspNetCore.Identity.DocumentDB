namespace IntegrationTests
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    public class RoleClaimStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task Create_NewRole_HasNoClaims()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);

            var claims = await manager.GetClaimsAsync(role);

            Assert.Empty(claims);
        }

        [Fact]
        public async Task AddClaim_ReturnsClaim()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);

            await manager.AddClaimAsync(role, new Claim("type", "value"));

            var claim = (await manager.GetClaimsAsync(role)).Single();
            Assert.Equal("type", claim.Type);
            Assert.Equal("value", claim.Value);
        }

        [Fact]
        public async Task RemoveClaim_RemovesExistingClaim()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);
            await manager.AddClaimAsync(role, new Claim("type", "value"));

            await manager.RemoveClaimAsync(role, new Claim("type", "value"));

            Assert.Empty(await manager.GetClaimsAsync(role));
        }

        [Fact]
        public async Task RemoveClaim_DifferentType_DoesNotRemoveClaim()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);
            await manager.AddClaimAsync(role, new Claim("type", "value"));

            await manager.RemoveClaimAsync(role, new Claim("otherType", "value"));

            Assert.NotEmpty(await manager.GetClaimsAsync(role));
        }

        [Fact]
        public async Task RemoveClaim_DifferentValue_DoesNotRemoveClaim()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);
            await manager.AddClaimAsync(role, new Claim("type", "value"));

            await manager.RemoveClaimAsync(role, new Claim("type", "otherValue"));

            Assert.NotEmpty(await manager.GetClaimsAsync(role));
        }
    }
}