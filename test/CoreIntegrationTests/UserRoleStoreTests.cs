namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;
    using System.Collections.Generic;

    // todo low - validate all tests work
    public class UserRoleStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task GetRoles_UserHasNoRoles_ReturnsNoRoles()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            var roles = await manager.GetRolesAsync(user);

            Assert.Empty(roles);
        }

        [Fact]
        public async Task AddRole_Adds()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AddToRoleAsync(user, "role");

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            // note: addToRole now passes a normalized role name
            Assert.Equal(new List<string> { "ROLE" }, savedUser.Roles);
            Assert.True(await manager.IsInRoleAsync(user, "role"));
        }

        [Fact]
        public async Task RemoveRole_Removes()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AddToRoleAsync(user, "role");

            await manager.RemoveFromRoleAsync(user, "role");

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.Empty(savedUser.Roles);
            Assert.False(await manager.IsInRoleAsync(user, "role"));
        }

        [Fact]
        public async Task GetUsersInRole_FiltersOnRole()
        {
            var roleA = "roleA";
            var roleB = "roleB";
            var userInA = new DocDBIdentity.IdentityUser { UserName = "nameA" };
            var userInB = new DocDBIdentity.IdentityUser { UserName = "nameB" };
            var manager = GetUserManager();
            await manager.CreateAsync(userInA);
            await manager.CreateAsync(userInB);
            await manager.AddToRoleAsync(userInA, roleA);
            await manager.AddToRoleAsync(userInB, roleB);

            var matchedUsers = await manager.GetUsersInRoleAsync("roleA");

            Assert.Single(matchedUsers);
            Assert.Equal("nameA", matchedUsers.First().UserName);
        }
    }
}