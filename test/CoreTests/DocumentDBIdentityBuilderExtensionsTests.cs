using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CoreTests
{
    using Microsoft.AspNetCore.Identity;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class DocumentDBIdentityBuilderExtensionsTests
    {
        private static readonly string databaseId = "AspDotNetCore.Identity.DocumentDB.Test";
        private static readonly string collectionId = "Users.Collection.Test";
        private static readonly string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        private DocumentClient client = new DocumentClient(new Uri("https://localhost:8081"), key);

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public void AddDocumentDBStores_WithDefaultTypesViaOptions_ResolvesStoresAndManagers()
        {
            var services = new ServiceCollection();
            services.AddIdentityWithDocumentDBStores(options =>
            {
                options.EnableDocumentDbEmulatorSupport();
                options.DatabaseId = databaseId;
                options.CollectionId = collectionId;
            });
            // note: UserManager and RoleManager use logging
            services.AddLogging();

            var provider = services.BuildServiceProvider();
            var resolvedUserStore = provider.GetService<IUserStore<DocDBIdentity.IdentityUser>>();
            Assert.NotNull(resolvedUserStore); // "User store did not resolve"

            var resolvedRoleStore = provider.GetService<IRoleStore<DocDBIdentity.IdentityRole>>();
            Assert.NotNull(resolvedRoleStore); // "Role store did not resolve"

            var resolvedUserManager = provider.GetService<UserManager<DocDBIdentity.IdentityUser>>();
            Assert.NotNull(resolvedUserManager); // "User manager did not resolve"

            var resolvedRoleManager = provider.GetService<RoleManager<DocDBIdentity.IdentityRole>>();
            Assert.NotNull(resolvedRoleManager); // "Role manager did not resolve"
        }

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public void AddDocumentDBStores_WithIdentityOptionsSetsThoseOptions()
        {
            var services = new ServiceCollection();
            services.AddIdentityWithDocumentDBStores(options =>
            {
                options.EnableDocumentDbEmulatorSupport();
                options.DatabaseId = databaseId;
                options.CollectionId = collectionId;
            }, identOptions =>
            {
                identOptions.ClaimsIdentity.UserNameClaimType = ClaimTypes.WindowsUserClaim;
            });
            // note: UserManager and RoleManager use logging
            services.AddLogging();
            services.AddOptions();

            var provider = services.BuildServiceProvider();
            var resolvedIdentOptions = provider.GetService<IOptions<IdentityOptions>>();

            Assert.NotNull(resolvedIdentOptions); // "Identity options did not resolve"
            Assert.Equal(ClaimTypes.WindowsUserClaim, resolvedIdentOptions.Value.ClaimsIdentity.UserNameClaimType); // "Identity options are not set"
        }

        protected class CustomUser : DocDBIdentity.IdentityUser
        {
        }

        protected class CustomRole : DocDBIdentity.IdentityRole
        {
        }

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public void AddDocumentDBStores_WithCustomTypesViaOptions_ThisShouldLookReasonableForUsers()
        {
            // this test is just to make sure I consider the interface for using custom types
            // so that it's not a horrible experience even though it should be rarely used
            var services = new ServiceCollection();

            services.AddIdentityWithDocumentDBStores<CustomUser, CustomRole>(options =>
            {
                options.EnableDocumentDbEmulatorSupport();
                options.DatabaseId = databaseId;
                options.CollectionId = collectionId;
            });
            services.AddLogging();

            var provider = services.BuildServiceProvider();
            var resolvedUserStore = provider.GetService<IUserStore<CustomUser>>();
            Assert.NotNull(resolvedUserStore); // "User store did not resolve"

            var resolvedRoleStore = provider.GetService<IRoleStore<CustomRole>>();
            Assert.NotNull(resolvedRoleStore); // "Role store did not resolve"

            var resolvedUserManager = provider.GetService<UserManager<CustomUser>>();
            Assert.NotNull(resolvedUserManager); // "User manager did not resolve"

            var resolvedRoleManager = provider.GetService<RoleManager<CustomRole>>();
            Assert.NotNull(resolvedRoleManager); // "Role manager did not resolve"
        }

        [Fact]
        public void AddDocumentDBStores_ConnectionStringWithoutDatabase_Throws()
        {
            DocumentClient client = null;

            string collectionId = "fake";

            var ex = Assert.Throws<ArgumentException>(() =>
            {
                new ServiceCollection()
                .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                {
                    options.EnableDocumentDbEmulatorSupport();
                    options.CollectionId = collectionId;
                });
            });
            Assert.Equal("DatabaseId cannot be null.", ex.Message);
        }

        protected class WrongUser : DocDBIdentity.IdentityUser
        {
        }

        protected class WrongRole : DocDBIdentity.IdentityRole
        {
        }

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public void AddDocumentDBStores_MismatchedTypes_ThrowsWarningToHelpUsers()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            {
                new ServiceCollection()
                    .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                    .RegisterDocumentDBStores<WrongUser, DocDBIdentity.IdentityRole>(options =>
                    {
                        options.EnableDocumentDbEmulatorSupport();
                        options.DatabaseId = databaseId;
                        options.CollectionId = collectionId;
                    });
            });
            Assert.Equal("User type passed to RegisterDocumentDBStores must match user type passed to AddIdentity. You passed Microsoft.AspNetCore.Identity.DocumentDB.DocDBIdentity.IdentityUser to AddIdentity and CoreTests.DocumentDBIdentityBuilderExtensionsTests+WrongUser to RegisterDocumentDBStores, these do not match.", ex.Message);

            var ex2 = Assert.Throws<ArgumentException>(() =>
            {
                new ServiceCollection()
                    .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                    .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, WrongRole>(options =>
                    {
                        options.EnableDocumentDbEmulatorSupport();
                        options.DatabaseId = databaseId;
                        options.CollectionId = collectionId;
                    });
            });
            Assert.Equal("Role type passed to RegisterDocumentDBStores must match role type passed to AddIdentity. You passed Microsoft.AspNetCore.Identity.DocumentDB.DocDBIdentity.IdentityRole to AddIdentity and CoreTests.DocumentDBIdentityBuilderExtensionsTests+WrongRole to RegisterDocumentDBStores, these do not match.", ex2.Message);
        }

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public async Task AddDocumentDBStores_NewAndExistingDatabase()
        {
            var database = client.CreateDatabaseQuery().Where(db => db.Id.Equals(databaseId))
                .AsEnumerable().FirstOrDefault();

            // make sure the database does not exist
            if (database != null)
            {
                database = await client.DeleteDatabaseAsync(database.SelfLink);
            }

            // when database does not exist does not throw:
            new ServiceCollection()
                .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                {
                    options.EnableDocumentDbEmulatorSupport();
                    options.DatabaseId = databaseId;
                    options.CollectionId = collectionId;
                });

            // when database exists does not throw:
            new ServiceCollection()
                .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                {
                    options.EnableDocumentDbEmulatorSupport();
                    options.DatabaseId = databaseId;
                    options.CollectionId = collectionId;
                });
        }

        [Fact]
        [Trait("Category", "BreaksUnix")]
        public async Task AddDocumentDBStores_NewAndExistingCollections()
        {
            var database = client.CreateDatabaseQuery().Where(db => db.Id.Equals(databaseId))
                .AsEnumerable().FirstOrDefault();

            // make sure the database exists
            if (database == null)
            {
                database = await client.CreateDatabaseAsync(new Database { Id = databaseId });
            }

            var collection = client.CreateDocumentCollectionQuery(database.SelfLink)
                .Where(c => c.Id.Equals(collectionId)).AsEnumerable().FirstOrDefault();

            // make sure the collection does not exist
            if (collection != null) { await client.DeleteDocumentCollectionAsync(collection.SelfLink); }

            // when collection does not exist does not throw:
            new ServiceCollection()
                .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                {
                    options.EnableDocumentDbEmulatorSupport();
                    options.DatabaseId = databaseId;
                    options.CollectionId = collectionId;
                });

            // when collection exists does not throw:
            new ServiceCollection()
                .AddIdentity<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>()
                .RegisterDocumentDBStores<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                {
                    options.EnableDocumentDbEmulatorSupport();
                    options.DatabaseId = databaseId;
                    options.CollectionId = collectionId;
                });
        }
    }
}