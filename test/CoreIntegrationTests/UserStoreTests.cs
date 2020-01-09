namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;
    using System;

    // todo low - validate all tests work
    public class UserStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task Create_NewUser_Saves()
        {
            var userName = "name";
            var user = new DocDBIdentity.IdentityUser { UserName = userName };
            var manager = GetUserManager();

            await manager.CreateAsync(user);

            var savedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.Equal(user.UserName, savedUser.UserName);
        }

        [Fact]
        public async Task FindByName_SavedUser_ReturnsUser()
        {
            var userName = "name";
            var user = new DocDBIdentity.IdentityUser { UserName = userName };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            var foundUser = await manager.FindByNameAsync(userName);

            Assert.NotNull(foundUser);
            Assert.Equal(userName, foundUser.UserName);
        }

        [Fact]
        public async Task FindByName_NoUser_ReturnsNull()
        {
            var manager = GetUserManager();

            var foundUser = await manager.FindByNameAsync("nouserbyname");

            Assert.Null(foundUser);
        }

        [Fact]
        public async Task FindById_SavedUser_ReturnsUser()
        {
            var userId = Guid.NewGuid().ToString();
            var user = new DocDBIdentity.IdentityUser { UserName = "name" };
            user.Id = userId;
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            var foundUser = await manager.FindByIdAsync(userId);

            Assert.NotNull(foundUser);
            Assert.Equal(userId, foundUser.Id);
        }

        [Fact]
        public async Task FindById_NoUser_ReturnsNull()
        {
            var manager = GetUserManager();

            var foundUser = await manager.FindByIdAsync(Guid.NewGuid().ToString());

            Assert.Null(foundUser);
        }

        [Fact]
        public async Task FindById_IdIsNotAnObjectId_ReturnsNull()
        {
            var manager = GetUserManager();

            var foundUser = await manager.FindByIdAsync("notanobjectid");

            Assert.Null(foundUser);
        }

        [Fact]
        public async Task Delete_ExistingUser_Removes()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "name" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            Assert.NotEmpty(Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable());

            await manager.DeleteAsync(user);

            Assert.Empty(Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable());
        }

        [Fact]
        public async Task Update_ExistingUser_Updates()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "name" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            var savedUser = await manager.FindByIdAsync(user.Id);
            savedUser.UserName = "newname";

            await manager.UpdateAsync(savedUser);

            var changedUser = Client.CreateDocumentQuery<DocDBIdentity.IdentityUser>(Users.DocumentsLink).AsEnumerable().FirstOrDefault();
            Assert.NotNull(changedUser);
            Assert.Equal("newname", changedUser.UserName);
        }

        [Fact]
        public async Task SimpleAccessorsAndGetters()
        {
            var user = new DocDBIdentity.IdentityUser
            {
                UserName = "username"
            };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            Assert.Equal(user.Id, await manager.GetUserIdAsync(user));
            Assert.Equal("username", await manager.GetUserNameAsync(user));

            await manager.SetUserNameAsync(user, "newUserName");
            Assert.Equal("newUserName", await manager.GetUserNameAsync(user));
        }
    }
}