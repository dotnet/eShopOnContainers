using eShopOnContainers.Core.Models.User;
using System;
using System.Threading.Tasks;

namespace eShopOnContainers.Core.Services.User
{
    public class UserMockService : IUserService
    {
        private UserInfo MockUserInfo = new UserInfo
        {
            UserId = Guid.NewGuid().ToString(),
            Name = "Jhon",
            LastName = "Doe",
            PreferredUsername = "Jdoe",
            Email = "jdoe@eshop.com",
            EmailVerified = true,
            PhoneNumber = "202-555-0165",
            PhoneNumberVerified = true,
            Address = "Seattle, WA",
            Street = "120 E 87th Street",
            ZipCode = "98101",
            Country = "United States",
            State = "Seattle",
            CardNumber = "378282246310005",
            CardHolder = "American Express",
            CardSecurityNumber = "1234"
        };

        public Task<UserInfo> GetUserInfoAsync(string authToken)
        {
            return Task.FromResult(MockUserInfo);
        }
    }
}