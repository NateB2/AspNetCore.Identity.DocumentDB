namespace IntegrationTests
{
    using System.Threading.Tasks;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Xunit;

    // todo low - validate all tests work
    public class UserPhoneNumberStoreTests : UserIntegrationTestsBase
    {
        private const string PhoneNumber = "1234567890";

        [Fact]
        public async Task SetPhoneNumber_StoresPhoneNumber()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);

            await manager.SetPhoneNumberAsync(user, PhoneNumber);

            Assert.Equal(PhoneNumber, await manager.GetPhoneNumberAsync(user));
        }

        [Fact]
        public async Task ConfirmPhoneNumber_StoresPhoneNumberConfirmed()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            var token = await manager.GenerateChangePhoneNumberTokenAsync(user, PhoneNumber);

            await manager.ChangePhoneNumberAsync(user, PhoneNumber, token);

            Assert.True(await manager.IsPhoneNumberConfirmedAsync(user));
        }

        [Fact]
        public async Task ChangePhoneNumber_OriginalPhoneNumberWasConfirmed_NotPhoneNumberConfirmed()
        {
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            var manager = GetUserManager();
            await manager.CreateAsync(user);
            var token = await manager.GenerateChangePhoneNumberTokenAsync(user, PhoneNumber);
            await manager.ChangePhoneNumberAsync(user, PhoneNumber, token);

            await manager.SetPhoneNumberAsync(user, PhoneNumber);

            Assert.False(await manager.IsPhoneNumberConfirmedAsync(user));
        }
    }
}