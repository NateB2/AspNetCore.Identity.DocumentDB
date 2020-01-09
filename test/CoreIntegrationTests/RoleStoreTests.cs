namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;
    using System;

    public class RoleStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task Create_NewRole_Saves()
        {
            var roleName = "admin";
            var role = new DocDBIdentity.IdentityRole(roleName);
            var manager = GetRoleManager();

            await manager.CreateAsync(role);

            var savedRole = Client.CreateDocumentQuery<DocDBIdentity.IdentityRole>(Roles.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.Equal(roleName, savedRole.Name);
            Assert.Equal("ADMIN", savedRole.NormalizedName);
        }

        [Fact]
        public async Task FindByName_SavedRole_ReturnsRole()
        {
            var roleName = "name";
            var role = new DocDBIdentity.IdentityRole { Name = roleName };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);

            // note: also tests normalization as FindByName now uses normalization
            var foundRole = await manager.FindByNameAsync(roleName);

            Assert.NotNull(foundRole);
            Assert.Equal(roleName, foundRole.Name);
        }

        [Fact]
        public async Task FindById_SavedRole_ReturnsRole()
        {
            var roleId = Guid.NewGuid().ToString();
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            role.Id = roleId;
            var manager = GetRoleManager();
            await manager.CreateAsync(role);

            var foundRole = await manager.FindByIdAsync(roleId);

            Assert.NotNull(foundRole);
            Assert.Equal(roleId, foundRole.Id);
        }

        [Fact]
        public async Task Delete_ExistingRole_Removes()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);
            Assert.NotEmpty(Client.CreateDocumentQuery<DocDBIdentity.IdentityRole>(Roles.DocumentsLink).AsEnumerable());

            await manager.DeleteAsync(role);

            Assert.Empty(Client.CreateDocumentQuery<DocDBIdentity.IdentityRole>(Roles.DocumentsLink).AsEnumerable());
        }

        [Fact]
        public async Task Update_ExistingRole_Updates()
        {
            var role = new DocDBIdentity.IdentityRole { Name = "name" };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);
            var savedRole = await manager.FindByIdAsync(role.Id);
            savedRole.Name = "newname";

            await manager.UpdateAsync(savedRole);

            var changedRole = Client.CreateDocumentQuery<DocDBIdentity.IdentityRole>(Roles.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.NotNull(changedRole);
            Assert.Equal("newname", changedRole.Name);
        }

        [Fact]
        public async Task SimpleAccessorsAndGetters()
        {
            var role = new DocDBIdentity.IdentityRole
            {
                Name = "name"
            };
            var manager = GetRoleManager();
            await manager.CreateAsync(role);

            Assert.Equal(role.Id, await manager.GetRoleIdAsync(role));
            Assert.Equal("name", await manager.GetRoleNameAsync(role));

            await manager.SetRoleNameAsync(role, "newName");
            Assert.Equal("newName", await manager.GetRoleNameAsync(role));
        }
    }
}