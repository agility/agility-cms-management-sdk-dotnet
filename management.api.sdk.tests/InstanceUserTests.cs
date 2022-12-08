using agility.models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using agility.utils;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace management.api.sdk.tests
{
    [TestClass]
    public class InstanceUserTests
    {
        ClientInstance clientInstance = null;
        private agility.models.Options _options;
        private AuthUtil _authUtil = null;

        public InstanceUserTests()
        {
            _authUtil = new AuthUtil();
            _options = _authUtil.GetTokenResponseData();
            clientInstance = new ClientInstance(_options);
        }

        [TestMethod]
        public async Task UserOperations()
        {
            try
            {
                string? guid = Environment.GetEnvironmentVariable("Guid");
                var userRole = new InstanceRole
                {
                    RoleID = 7,
                    IsGlobalRole = true,
                    Sort = 0,
                    Role = "Administrator",
                    Name = "Administrator"
                };

                List<InstanceRole> userRoles = new List<InstanceRole>();
                userRoles.Add(userRole);
                var firstName = $"FirstName{DateTime.Now.Ticks}";
                var lastName = $"LastName{DateTime.Now.Ticks}";
                var email = $"{firstName}.{lastName}@mail.com";

                var savedUser = await clientInstance.instanceUserMethods.SaveUser(email, userRoles, guid, firstName, lastName);
                Assert.IsNotNull(savedUser, $"Unable to save user with email: {email}, firstName: {firstName}, lastName {lastName}");

                var userDelete = await clientInstance.instanceUserMethods.DeleteUser(savedUser.UserID, guid);
                Assert.IsNotNull(userDelete, $"Unable to delete user with userID {savedUser.UserID}");

                var users = await clientInstance.instanceUserMethods.GetUsers(guid);
                Assert.IsNotNull(users, "Unable to retrieve users.");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            
        }
    }
}
