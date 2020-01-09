namespace IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using DocDBIdentity = Microsoft.AspNetCore.Identity.DocumentDB;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    // todo low - validate all tests work
    public class UserLockoutStoreTests : UserIntegrationTestsBase
    {
        [Fact]
        public async Task AccessFailed_IncrementsAccessFailedCount()
        {
            var manager = GetUserManagerWithThreeMaxAccessAttempts();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AccessFailedAsync(user);

            Assert.Equal(1, await manager.GetAccessFailedCountAsync(user));
        }

        private UserManager<DocDBIdentity.IdentityUser> GetUserManagerWithThreeMaxAccessAttempts()
        {
            return CreateServiceProvider<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options => options.Lockout.MaxFailedAccessAttempts = 3)
                .GetService<UserManager<DocDBIdentity.IdentityUser>>();
        }

        [Fact]
        public void IncrementAccessFailedCount_ReturnsNewCount()
        {
            /* TODO
			var store = new UserStore<IdentityUser>(documentClient, null);
			var user = new DocDBIdentity.IdentityUser {UserName = "bob"};

			var count = store.IncrementAccessFailedCountAsync(user, default(CancellationToken));

			Expect(count.Result, Is.EqualTo(1));
            */
        }

        [Fact]
        public async Task ResetAccessFailed_AfterAnAccessFailed_SetsToZero()
        {
            var manager = GetUserManagerWithThreeMaxAccessAttempts();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);
            await manager.AccessFailedAsync(user);

            await manager.ResetAccessFailedCountAsync(user);

            Assert.Equal(0, await manager.GetAccessFailedCountAsync(user));
        }

        [Fact]
        public async Task AccessFailed_NotOverMaxFailures_NoLockoutEndDate()
        {
            var manager = GetUserManagerWithThreeMaxAccessAttempts();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AccessFailedAsync(user);

            Assert.Null(await manager.GetLockoutEndDateAsync(user));
        }

        [Fact]
        public async Task AccessFailed_ExceedsMaxFailedAccessAttempts_LocksAccount()
        {
            var manager = CreateServiceProvider<DocDBIdentity.IdentityUser, DocDBIdentity.IdentityRole>(options =>
                 {
                     options.Lockout.MaxFailedAccessAttempts = 0;
                     options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(1);
                 })
                .GetService<UserManager<DocDBIdentity.IdentityUser>>();

            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.AccessFailedAsync(user);

            var lockoutEndDate = await manager.GetLockoutEndDateAsync(user);
            Assert.InRange(lockoutEndDate.Value.Subtract(DateTime.UtcNow).TotalHours, 0.9, 1.1); // TODO check if this is the same as before!!
        }

        [Fact]
        public async Task SetLockoutEnabled()
        {
            var manager = GetUserManager();
            var user = new DocDBIdentity.IdentityUser { UserName = "bob" };
            await manager.CreateAsync(user);

            await manager.SetLockoutEnabledAsync(user, true);
            Assert.True(await manager.GetLockoutEnabledAsync(user));

            await manager.SetLockoutEnabledAsync(user, false);
            Assert.False(await manager.GetLockoutEnabledAsync(user));
        }
    }
}