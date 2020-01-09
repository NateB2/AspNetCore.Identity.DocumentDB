namespace IntegrationTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    public class DocumentDBCollectionTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task CollectionWithMultipleDocumentTypes_Works()
        {
            // creates user and role and checks that they do not interfere with eachother
            // as both are stored in the same DocumentDB collection

            // create user
            var userName = "admin";
            var user = new DocDBIdentity.IdentityUser { UserName = userName };
            var userManager = GetUserManager();

            await userManager.CreateAsync(user);

            // create role
            var roleName = "admin";
            var role = new DocDBIdentity.IdentityRole(roleName);
            var roleManager = GetRoleManager();

            await roleManager.CreateAsync(role);

            // check query on collection
            var results = Client.CreateDocumentQuery(Users.DocumentsLink).ToList();
            Assert.Equal(2, results.Count);

            // .NET Identity Framework - get all results
            var userList = userManager.Users.ToList();
            Assert.Single(userList);
            Assert.Equal(user.Id, userList[0].Id);

            var roleList = roleManager.Roles.ToList();
            Assert.Single(roleList);
            Assert.Equal(role.Id, roleList[0].Id);

            // .NET Identity Framework - get specific result
            var foundUser = await userManager.FindByNameAsync(userName);
            Assert.Equal(userName, foundUser.UserName);
            Assert.Equal(user.Id, foundUser.Id);

            var foundRole = await roleManager.FindByNameAsync(roleName);
            Assert.Equal(roleName, foundRole.Name);
            Assert.Equal(role.Id, foundRole.Id);

            Assert.NotEqual(foundRole.Id, foundUser.Id);
        }
    }
}