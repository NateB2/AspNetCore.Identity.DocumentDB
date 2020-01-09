using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace IntegrationTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Extensions.DependencyInjection;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;

    public class UserIntegrationTestsBase
    {
        protected DocumentClient Client;
        protected Database Database;
        protected DocumentCollection Users;
        protected DocumentCollection Roles;

        // note: for now we'll have interfaces to both the new and old apis for DocumentDB, that way we don't have to update all the tests at once and risk introducing bugs
        protected IServiceProvider ServiceProvider;

        // Default settings to connect with DocumentDB Emulator:
        private const string endpointUrl = "https://localhost:8081";

        private const string primaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private static readonly string databaseName = "AspDotNetCore.Identity.DocumentDB.Test";

        public UserIntegrationTestsBase(PartitionKeyDefinition partitionKey = null)
        {
            BeforeEachTest(partitionKey).Wait();
        }

        public async Task BeforeEachTest(PartitionKeyDefinition partitionKey = null)
        {
            Client = new DocumentClient(new Uri(endpointUrl), primaryKey, connectionPolicy: ConnectionPolicy.Default);

            try
            {
                Database = await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
            }
            catch (DocumentClientException exception)
            {
                if (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    Database = (await Client.CreateDatabaseAsync(new Database { Id = databaseName })).Resource;
                }
                else
                {
                    throw;
                }
            }

            var collectionName = "users";
            if (partitionKey != null)
            {
                collectionName += "-partitioned";
            }

            Users = Client.CreateDocumentCollectionQuery(Database.SelfLink)
                .Where(c => c.Id.Equals(collectionName)).AsEnumerable().FirstOrDefault();
            Roles = Users;

            if (Users != null) { await Client.DeleteDocumentCollectionAsync(Users.SelfLink); }

            var collection = new DocumentCollection { Id = collectionName };
            if (partitionKey != null)
            {
                collection.PartitionKey = partitionKey;
            }

            Users = await Client.CreateDocumentCollectionAsync(Database.SelfLink, collection);
            Roles = Users;

            ServiceProvider = CreateServiceProvider<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>();
        }

        protected UserManager<DocDBIdentity.IdentityUser> GetUserManager()
            => ServiceProvider.GetService<UserManager<DocDBIdentity.IdentityUser>>();

        protected RoleManager<DocDBIdentity.IdentityRole> GetRoleManager()
            => ServiceProvider.GetService<RoleManager<DocDBIdentity.IdentityRole>>();

        protected IServiceProvider CreateServiceProvider<TUser, TRole>(Action<IdentityOptions> optionsProvider = null)
            where TUser : DocDBIdentity.IdentityUser
            where TRole : DocDBIdentity.IdentityRole
        {
            var services = new ServiceCollection();
            optionsProvider = optionsProvider ?? (options => { });
            services.AddIdentity<TUser, TRole>(optionsProvider)
                .AddDefaultTokenProviders()
                .RegisterDocumentDBStores<TUser, TRole>(Client, (p) => Users);

            services.AddLogging();

            return services.BuildServiceProvider();
        }
    }
}