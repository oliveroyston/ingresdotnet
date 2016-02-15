#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : TestIngresRoleProvider.cs
 *
 * Copyright            : (c) Oliver P. Oyston 2008
 * 
 * Licence              : GNU Lesser General Public Licence
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the 
 * GNU Lesser General Public License as published by the Free Software Foundation, either version 3
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See 
 * the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with this 
 * program. If not, see <http://www.gnu.org/licenses/>.
 * 
 * Version History
 *
 * Version  Date        Who     Description
 * -------  ----------  ---     --------------
 * 1.0      01/10/2008  opo     Original Version
*/

#endregion

namespace TestIngresAspNetProviders
{
    #region NameSpaces Used

    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Data;
    using System.Text;
    using System.Web.Security;
    using DbUnit.Dataset;
    using DbUnit.Framework;
    using Ingres.Client;
    using NUnit.Framework;

    #endregion

    #region Test Fixture for the Ingres Role Provider

    /// <summary>
    /// <param>Test fixture for the Ingres ASP.NET Role Provider.</param>
    /// <param>We are using DBUnit so that the database is automatically populated with known data at the
    /// start of each test and then emptied of the data at the end of each test.</param>
    /// <param>Please note: some of the exceptions are thrown by the ASP.NET Framework BEFORE the Ingres
    /// Role Provider methods are even called - hence the expected exceptions are the exceptions
    /// from the framework and not the Ingres Role provider. However, as a belt-and-braces check, 
    /// the Ingres Role Provider should validate all of its input parameters in case it is directly 
    /// instantiated.</param>
    /// <param>As the Ingres Role Provider works in its own transactional state we have to do clean up
    /// in the finally block of some tests. The ValidateDatabaseIsEmpty() method ensures that 
    /// before we start any test case (and after we have finished tearing down a test case) that 
    /// the database is as we expect.</param>
    /// </summary>
    [TestFixture]
    public class TestIngresRoleProvider : DbUnitTestCase
    {
        #region Constants

        /// <summary>
        /// String value to use as the category for these tests.
        /// </summary>
        private const string GC_CATEGORY_ROLE_PROVIDER = "Ingres ASP.NET Role Provider Tests";
   
        /// <summary>
        /// Error message for the exceptions that are thrown when we encounter an error in
        /// validating.
        /// </summary>
        private const string GC_VALIDATION_ERROR = "An error occurred during validation";

        /// <summary>
        /// The name of a role that does not exist in the database.
        /// </summary>
        private const string GC_NON_EXISTENT_ROLE = "NonExistentRole";

        /// <summary>
        /// The name of a user that does not exist in the database.
        /// </summary>
        private const string GC_NON_EXISTENT_USER = "NonExistentUser";

        /// <summary>
        /// A user name with a comma in.
        /// </summary>
        private const string GC_USER_NAME_WITH_COMMA = "User,Name";

        /// <summary>
        /// A role name with a comma in.
        /// </summary>
        private const string GC_ROLE_NAME_WITH_COMMA = "Role,Name";

        /// <summary>
        /// The name of an unpopulated role.
        /// </summary>
        private const string GC_UNPOPULATED_ROLE = "UnpopulatedRole";

        /// <summary>
        /// The name of a populated role.
        /// </summary>
        private const string GC_POPULATED_ROLE = "PopulatedRole"; 

        /// <summary>
        /// The name of a user without any roles.
        /// </summary>
        private const string GC_USER_WITH_NO_ROLES = "UserWithNoRoles";

        /// <summary>
        /// The name of a role that exists,
        /// </summary>
        private const string GC_EXISTING_ROLE = "RoleName1";

        #endregion

        #region Private Fields

        /// <summary>
        /// A connection to the Ingres database.
        /// </summary>
        private IngresConnection connection;

        #endregion

        #region Getters and Setters

        /// <summary>
        /// Gets or sets the Connection.
        /// </summary>
        public IngresConnection Connection
        {
            get
            {
                return this.connection;
            }

            set
            {
                this.connection = value;
            }
        }

        #endregion

        #region Ingres Role Provider Tests

        #region CreateRole Tests

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role Provider passing in a valid role name 
        /// that does not already exist for the application.
        /// </summary>
        /// <expectedResult>
        /// A role is successfully created.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestCreateRole()
        {
            bool roleExists;

            // Ensure that the role doesn't already exist
            try
            {
                roleExists = Roles.RoleExists(GC_NON_EXISTENT_ROLE);
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            Assert.AreEqual(false, roleExists);

            // Create the new role
            Roles.CreateRole(GC_NON_EXISTENT_ROLE);

            // Validate the creation
            try
            {
                roleExists = Roles.RoleExists(GC_NON_EXISTENT_ROLE);
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }

            Assert.AreEqual(true, roleExists);
            
            // Clean up...
            try
            {
                // The Role provider operates in a different transaction and hence we need to
                // explicitly delete the role that it may have created (if it exists). 
                if (Roles.RoleExists(GC_NON_EXISTENT_ROLE))
                {
                    Roles.DeleteRole(GC_NON_EXISTENT_ROLE);

                    Assert.AreEqual(false, Roles.RoleExists(GC_NON_EXISTENT_ROLE));
                }
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role Provider passing in a role that
        /// already exists in the database for the current application.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'RoleName1' already exists.")]
        [Test]
        public void TestCreateRoleWithRoleAlreadyExisting()
        {
            // Attempt to create the new role (that already existing in the database).
            Roles.CreateRole(GC_EXISTING_ROLE);
        }

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role Provider passing in null for the role name.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestCreateRoleWithNullRole()
        {
            // Attempt to create the new role.
            Roles.CreateRole(null);
        }

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role provider passing in an empty string for 
        /// the role.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestCreateRoleWithEmptyRole()
        {
            // Attempt to create the new role.
            Roles.CreateRole(string.Empty);
        }

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role Provider and passing in a role
        /// name containing a comma.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestCreateRoleWithRolenameContainingComma()
        {
            // Attempt to create the new role.
            Roles.CreateRole(GC_ROLE_NAME_WITH_COMMA);
        }

        /// <summary>
        /// Tests the calling of the CreateRole method of the Role Provider and passing in a role
        /// with a length exceeding the maximum allowed length for a role.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown indicating that the name is too long..
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestCreateRoleWithRolenameExceedingMaximumAllowedLength()
        {
            // Construct a long role name
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < 1000; i++)
            {
                sb.Append("x");
            }

            // Attempt to create the role.
            Roles.CreateRole(sb.ToString());
        }

        #endregion

        #region DeleteRole Tests

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role provider passing in the name
        /// of a role that exists for the application and that does not have any users in the role.
        /// </summary>
        /// <expectedResult>
        /// The role is successfully deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestDeleteRole()
        {
            // Validate that the role exists before we start
            try
            {
                Assert.AreEqual(true, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);                    
            }

            // Delete the Role
            Roles.DeleteRole(GC_UNPOPULATED_ROLE);

            // Validate that the role no longer exists
            try
            {
                Assert.AreEqual(false, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // DBUnit would have deleted the role after this test case anyway and so we don't need
            // to take any compensatory measures here.
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider passing in the name of
        /// a role that does not exist for the application.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is throw that indicates that the role does not exist.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        [Test]
        public void TestDeleteRoleWithRoleNotExisting()
        {
            // Attempt to delete the role
            Roles.DeleteRole(GC_NON_EXISTENT_ROLE);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider passing in the name of
        /// a role that does that exist for the application and has users assigned to the role.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown indicating that a populated role can not be deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "This role cannot be deleted because there are users present in it.")]
        public void TestDeleteRoleWithPopulatedRole()
        {
            // Attempt to delete a populated role.
            Roles.DeleteRole(GC_POPULATED_ROLE);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider passing in a role name that contains a 
        /// comma.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestDeleteRoleWithCommaInName()
        {
            // Attempt to delete a populated role.
            Roles.DeleteRole(GC_ROLE_NAME_WITH_COMMA);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider passing in an empty string as the name 
        /// of the role.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestDeleteEmptyStringRole()
        {
            // Attempt to delete a populated role.
            Roles.DeleteRole(string.Empty);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider passing in null as the name of the
        /// role.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestDeleteNullRole()
        {
            // Attempt to delete a populated role.
            Roles.DeleteRole(null);
        }

        #endregion

        #region DeleteRoleThrowOnPopulated Tests

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the 
        /// name of a role that exists (with no users in the role) and indicating that we shouldn't
        /// throw an error with a populated role. 
        /// </summary>
        /// <expectedResult>
        /// The role is successfully deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestDeleteRoleThrowOnPopulatedFalse()
        {
            // Validate that the role exists before we start
            try
            {
                Assert.AreEqual(true, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Delete the Role
            Roles.DeleteRole(GC_UNPOPULATED_ROLE, false);

            // Validate that the role no longer exists
            try
            {
                Assert.AreEqual(false, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the name 
        /// of a role that does not exist and indicating that we shouldn't throw an error with a populated 
        /// role. 
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown indicating that the role does not exist
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        [Test]
        public void TestDeleteRoleThrowOnPopulatedFalseWithRoleNotExisting()
        {
            Roles.DeleteRole(GC_NON_EXISTENT_ROLE, false);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the name 
        /// of a role that that eists and that is populated and indicating that we shouldn't throw 
        /// an error with a populated role. 
        /// </summary>
        /// <expectedResult>
        /// The role is successfully deleted and all users are removed from the role
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestDeleteRoleThrowOnPopulatedFalseWithPopulatedRole()
        {
            // Validate that the role exists before we start
            try
            {
                Assert.AreEqual(true, Roles.RoleExists(GC_POPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Rolename4 has a user in the role
            Roles.DeleteRole(GC_POPULATED_ROLE, false);

            // Validate that the role has been deleted and that the role no longer has any users
            try
            {
                Assert.AreEqual(false, Roles.RoleExists(GC_POPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        // Note: The following tests in this region are essentially the same as the DeleteRoleTests 
        // as the same method is called (in the provider)for both! I've included them though in case 
        // the Role provider implementation is ever changed.

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the 
        /// name of a role that exists (with no users in the role) and indicating that we should
        /// throw an error with a populated role. 
        /// </summary>
        /// <expectedResult>
        /// The role is successfully deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestDeleteRoleThrowOnPopulatedTrue()
        {
            // Validate that the role exists before we start
            try
            {
                Assert.AreEqual(true, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Delete the Role
            Roles.DeleteRole(GC_UNPOPULATED_ROLE, true);

            // Validate that the role no longer exists
            try
            {
                Assert.AreEqual(false, Roles.RoleExists(GC_UNPOPULATED_ROLE));
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the name 
        /// of a role that does not exist and indicating that we should throw an error with a populated 
        /// role. 
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown indicating that the role does not exist
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        [Test]
        public void TestDeleteRoleThrowOnPopulatedTrueWithRoleNotExisting()
        {
            Roles.DeleteRole(GC_NON_EXISTENT_ROLE, true);
        }

        /// <summary>
        /// Tests the calling of the DeleteRole method of the Role Provider and specifing the name 
        /// of a role that that exists and that is populated and indicating that we should throw 
        /// an error with a populated role. 
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that we can not deleted a populated role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "This role cannot be deleted because there are users present in it.")]
        public void TestDeleteRoleThrowOnPopulatedTrueWithPopulatedRole()
        {
            // Rolename4 has a user in the role
            Roles.DeleteRole(GC_POPULATED_ROLE, true);
        }
        #endregion

        #region GetAllRoles Tests
        
        /// <summary>
        /// Tests the calling of the GetAllRoles method of the Role Provider for an application
        /// that has roles.
        /// </summary>
        /// <expectedResult>
        /// All roles for the application are returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetAllRoles()
        {
            string[] roles = Roles.GetAllRoles();

            // We should have four roles
            Assert.AreEqual(16, roles.Length);

            // We don't know the order that the roles will come back so add them all to a list
            // and then check that the expected roles are in the list.
            List<string> list = new List<string>();

            foreach (string role in roles)
            {
                list.Add(role);
            }

            // Check that we have the roles that we expected and in the order that we
            // expected.
            Assert.IsTrue(list.Contains("Rolename1"));
            Assert.IsTrue(list.Contains("Rolename2"));
            Assert.IsTrue(list.Contains("Rolename3"));
            Assert.IsTrue(list.Contains("PopulatedRole"));
            Assert.IsTrue(list.Contains("GetRolesForUserRole1"));
            Assert.IsTrue(list.Contains("GetRolesForUserRole2"));
            Assert.IsTrue(list.Contains("GetRolesForUserRole3"));
            Assert.IsTrue(list.Contains("GetRolesForUserRole4"));
            Assert.IsTrue(list.Contains("UnpopulatedRole"));
            Assert.IsTrue(list.Contains("RoleForGetUsersInRole"));
            Assert.IsTrue(list.Contains("MultipleRole1"));
            Assert.IsTrue(list.Contains("MultipleRole2"));
            Assert.IsTrue(list.Contains("MultipleRole3"));
            Assert.IsTrue(list.Contains("UsersInRolesRole1"));
            Assert.IsTrue(list.Contains("UsersInRolesRole2"));
            Assert.IsTrue(list.Contains("UsersInRolesRole3"));      
        }

        /// <summary>
        /// Tests the calling of the GetAllRoles method of the Role Provider for an application
        /// that does not have any roles.
        /// </summary>
        /// <expectedResult>
        /// An empty string array is returned.
        /// </expectedResult>        
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetAllRolesWithNoRolesInSystem()
        {
            try
            {
                // Delete all of the roles that have been added as test data by DBUnit.
                Roles.DeleteRole("Rolename1", false);
                Roles.DeleteRole("Rolename2", false);
                Roles.DeleteRole("Rolename3", false);
                Roles.DeleteRole("PopulatedRole", false);
                Roles.DeleteRole("GetRolesForUserRole1", false);
                Roles.DeleteRole("GetRolesForUserRole2", false);
                Roles.DeleteRole("GetRolesForUserRole3", false);
                Roles.DeleteRole("GetRolesForUserRole4", false);
                Roles.DeleteRole("UnpopulatedRole", false);
                Roles.DeleteRole("RoleForGetUsersInRole", false);
                Roles.DeleteRole("MultipleRole1", false);
                Roles.DeleteRole("MultipleRole2", false);
                Roles.DeleteRole("MultipleRole3", false);
                Roles.DeleteRole("UsersInRolesRole1", false);
                Roles.DeleteRole("UsersInRolesRole2", false);
                Roles.DeleteRole("UsersInRolesRole3", false);
            }
            catch (Exception)
            {
                // Throw any errors created during validation
                throw new Exception(GC_VALIDATION_ERROR);                
            }

            // Get all roles
            string[] roles = Roles.GetAllRoles();

            // We shouldn't have any roles
            Assert.That(roles.Length == 0);
        }

        #endregion

        #region Adding Users To Roles Tests

        #region AddUserToRole Tests

        /// <summary>
        /// Tests a call to the AddUserToRoles method of the Ingres Role Provider passing in a 
        /// valid username and a valid rolenames with the user not already in the role.
        /// </summary>
        /// <expectedResult>
        /// The user is added to the role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestAddUserToRole()
        {
            try
            {
                // Validate that the user are not already in the role
                Assert.IsFalse(Roles.IsUserInRole("Username1", "Rolename1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Call the role provider so that the user is added role
            Roles.AddUserToRole("Username1", "Rolename1");

            try
            {
                // Validate that the user has successfully been added to the role.
                Assert.IsTrue(Roles.IsUserInRole("Username1", "Rolename1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Now, as a compensatory transaction we must remove the user from the role again
            Roles.RemoveUserFromRole("Username1", "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUserToRole method of the Ingres Role Provider passing in a 
        /// valid role but a username with a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRoleWithCommaInName()
        {
            // Call the role provider
            Roles.AddUserToRole("Userna,me1", "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// null for the username but a valid rolename.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUserToRoleWithNullInName()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole(null, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUserToRole method of the Ingres Role Provider passing in an
        /// empty string for the username and a valid rolename.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRoleWithEmptyStringInName()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole(string.Empty, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// null role but a valid username.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUserToRoleWithNullInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole("Username1", null);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in an
        /// empty string for the rolename and a valid username.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRoleWithEmptyStringInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole("Username1", string.Empty);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// valid role and username but the user is already in the role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'UserInARole' is already in role 'PopulatedRole'.")]
        public void TestAddUserToRoleWithAUserAlreadyInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole("UserInARole", "PopulatedRole");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// non-existent user and a valid role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentUser' was not found.")]
        public void TestAddUserToRoleWithUsernameNotExisting()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole(GC_NON_EXISTENT_USER, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// valid user but a non-existent role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        public void TestAddUserToRoleWithRolenameNotExisting()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRole("Username1", GC_NON_EXISTENT_ROLE);
        }

        #endregion

        #region AddUserToRoles Tests

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestAddUserToRoles()
        {
            try
            {
                // Validate that the users are not already in the roles
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename2"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);

            try
            {
                // Validate that the user has successfully been added to the roles.
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename2"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Now, as a compensatory transaction we must remove the users from the roles again
            Roles.RemoveUserFromRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRolesWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "Rolename1", GC_ROLE_NAME_WITH_COMMA, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUserToRolesWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] rolenames = new string[] { "Rolename1", "Rolename2", null };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRolesWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles(string.Empty, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUserToRolesWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] rolenames = new string[] { "Rolename1", null, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRolesWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] rolenames = new string[] { "Rolename1", string.Empty, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRolesWithAUserAlreadyInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3, Rolename4" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUserToRolesWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3, Rolename4" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles(GC_NON_EXISTENT_USER, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'InvalidRolename' was not found.")]
        public void TestAddUserToRolesWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "InvalidRolename", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUserToRoles("Username1", rolenames);
        }

        #endregion

        #region AddUsersToRoles Tests

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestAddUsersToRoles()
        {
            try
            {
                // Validate that the users are not already in the roles
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename2"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename3"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username2", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username2", "Rolename2"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username2", "Rolename3"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username3", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username3", "Rolename2"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username3", "Rolename3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);

            try
            {
                // Validate that the users have successfully been added to the roles.
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename2"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename3"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username2", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username2", "Rolename2"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username2", "Rolename3"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username3", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username3", "Rolename2"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username3", "Rolename3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Now, as a compensatory transaction we must remove the users from the roles again
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRolesWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUsersToRolesWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] usernames = new string[] { "Username1", null, "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRolesWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] usernames = new string[] { "Username1", string.Empty, "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUsersToRolesWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", null, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRolesWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", string.Empty, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRolesWithAUserAlreadyInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3, Rolename4" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRolesWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", "InvalidUsername" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3, Rolename4" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'InvalidRolename' was not found.")]
        public void TestAddUsersToRolesWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "InvalidRolename", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRoles(usernames, rolenames);
        }

        #endregion

        #region AddUsersToRole Tests

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestAddUsersToRole()
        {
            try
            {
                // Validate that the users are not already in the role
                Assert.AreEqual(false, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username2", "Rolename1"));
                Assert.AreEqual(false, Roles.IsUserInRole("Username3", "Rolename1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");

            try
            {
                // Validate that the users have successfully been added to the roles.
                Assert.AreEqual(true, Roles.IsUserInRole("Username1", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username2", "Rolename1"));
                Assert.AreEqual(true, Roles.IsUserInRole("Username3", "Rolename1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Now, as a compensatory transaction we must remove the users from the roles again
            Roles.RemoveUsersFromRole(usernames, "Rolename1");
        }
        
        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRoleWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestAddUsersToRoleWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] usernames = new string[] { "Username1", null, "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRoleWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] usernames = new string[] { "Username1", string.Empty, "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRoleWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestAddUsersToRoleWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, string.Empty);
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'UserInARole' is already in role 'PopulatedRole'.")]
        public void TestAddUsersToRoleWithAUserAlreadyInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", "UserInARole" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "PopulatedRole");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentUser' was not found.")]
        public void TestAddUsersToRoleWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", GC_NON_EXISTENT_USER };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the AddUsersToRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        public void TestAddUsersToRoleWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.AddUsersToRole(usernames, GC_NON_EXISTENT_ROLE);
        }

        #endregion

        #endregion

        #region GetRolesForUser Tests

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in the
        /// name of a user who exists and has roles assigned.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetRolesForUser()
        {
            // Get the roles for the user
            string[] roles = Roles.GetRolesForUser("GetRolesForUserUser");

            // Check that we have retrieved the number of roles that we expect
            Assert.That(roles.Length == 4);

            // We don't know the order that the roles will come back and hence we add them add
            // into a list
            List<string> list = new List<string>();

            foreach (string role in roles)
            {
                list.Add(role);
            }

            // Ensure that all epected roles are in the list
            Assert.That(list.Contains("GetRolesForUserRole1"));
            Assert.That(list.Contains("GetRolesForUserRole2"));
            Assert.That(list.Contains("GetRolesForUserRole3"));
            Assert.That(list.Contains("GetRolesForUserRole4")); 
        }

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in the
        /// name of a user who exists but who does not have any roles assigned.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetRolesForUserWithNoRolesAssigned()
        {
            string[] roles = Roles.GetRolesForUser(GC_USER_WITH_NO_ROLES);

            Assert.That(roles.Length == 0);
        }

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in the
        /// name of a non existent user.
        /// </summary>
        /// <remarks>
        /// A provider exception is thrown that indicated that the user does not exist. 
        /// </remarks>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user was not found in the database.")]
        [Test]
        public void TestGetRolesForUserWithInvalidUser()
        {
            Roles.GetRolesForUser(GC_NON_EXISTENT_USER);
        }

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in the
        /// name of a user with a comma.
        /// </summary>
        /// <remarks>
        /// A provider exception is thrown that indicated that the user does not exist. 
        /// </remarks>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestGetRolesForUserWithUserNameWithComma()
        {
            Roles.GetRolesForUser(GC_USER_NAME_WITH_COMMA);
        }

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in an
        /// empty string for the name of the user. It appears that the Ingres Role Provider is 
        /// not even called - ASP.NET simply returns an empty array.
        /// </summary>
        /// <remarks>
        /// An empty string array is removed. 
        /// </remarks>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetRolesForUserWithEmptyStringUser()
        {
            string[] roles = Roles.GetRolesForUser(string.Empty);

            Assert.That(roles.Length == 0);
        }

        /// <summary>
        /// Test the calling of the GetRolesForUser method of the Role Provider passing in null for
        /// the name of the user.
        /// </summary>
        /// <remarks>
        /// A provider exception is thrown that indicated that the user does not exist. 
        /// </remarks>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestGetRolesForUserWithNullUser()
        {
            Roles.GetRolesForUser(null);
        }

        #endregion

        #region GetUsersInRole Tests

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is populated.
        /// </summary>
        /// <expectedResult>
        /// A successfull return of all users in the role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetUsersInRole()
        {
            string[] users = Roles.GetUsersInRole("RoleForGetUsersInRole");

            Assert.That(users.Length == 3);

            List<string> list = new List<string>();

            foreach (string user in users)
            {
                list.Add(user);
            }

            Assert.That(list.Contains("GetUsersInRoleUser1"));
            Assert.That(list.Contains("GetUsersInRoleUser2"));
            Assert.That(list.Contains("GetUsersInRoleUser3"));
        }

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is unpopulated.
        /// </summary>
        /// <expectedResult>
        /// An empty string array is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestGetUsersInRoleWhenNoUsersInRole()
        {
            string[] users = Roles.GetUsersInRole(GC_UNPOPULATED_ROLE);

            Assert.That(users.Length == 0);
        }

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in the name
        /// of a role that does not exist.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException))]
        [Test]
        public void TestGetUsersInRoleInvalidRole()
        {
            Roles.GetUsersInRole(GC_NON_EXISTENT_ROLE);
        }

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in the name
        /// of an role that has a comma in.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestGetUsersInRoleWithCommaInRole()
        {
            Roles.GetUsersInRole(GC_USER_NAME_WITH_COMMA);
        }

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in null as
        /// the name of the role.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestGetUsersInRoleWithNullRole()
        {
            Roles.GetUsersInRole(null);
        }

        /// <summary>
        /// Tests the calling of the GetUsersInRole method of the Role Provider passing in an empty
        /// string as the name of the role.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestGetUsersInRoleWithEmptyStringRole()
        {
            Roles.GetUsersInRole(string.Empty);
        }

        #endregion

        #region RoleExists Tests

        /// <summary>
        /// Tests the calling of the RoleExists method of the provider passing in a role that
        /// does exist.
        /// </summary>
        /// <expectedResult>
        /// RoleExists returns true.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRoleExistsWithExistingRole()
        {
            // Check if the role exists
            bool roleExists = Roles.RoleExists(GC_EXISTING_ROLE);

            // The role should exist
            Assert.AreEqual(true, roleExists);
        }

        /// <summary>
        /// Tests the calling of the RoleExists method of the provider passing in a role that
        /// does not exist.
        /// </summary>
        /// <expectedResult>
        /// RoleExists returns false
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRoleExistsWithNonExistentRole()
        {
            // Check if the role exists
            bool roleExists = Roles.RoleExists(GC_NON_EXISTENT_ROLE);

            // The role should not exist
            Assert.AreEqual(false, roleExists);
        }

        /// <summary>
        /// Tests the calling of the RoleExists method of the provider passing in an empty string
        /// for the role name.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestRoleExistsWithCommaInRolename()
        {
            // Check if the role exists
            Roles.RoleExists(GC_ROLE_NAME_WITH_COMMA);
        }

        /// <summary>
        /// Tests the calling of the RoleExists method of the provider passing in an empty string
        /// for the role.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestRoleExistsWithEmptyStringForRole()
        {
            // Check if the role exists
            Roles.RoleExists(string.Empty);
        }

        /// <summary>
        /// Tests the calling of the RoleExists method of the provider passing in null for the role
        /// name.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestRoleExistsWithNullForRole()
        {
            // Check if the role exists
            Roles.RoleExists(null);
        }

        #endregion

        #region FindUsersInRole

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is populated.
        /// </summary>
        /// <expectedResult>
        /// A successfull return of all users in the role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestFindUsersInRole()
        {
            string[] users = Roles.FindUsersInRole("RoleForGetUsersInRole", "%");

            Assert.That(users != null);

            Assert.That(users.Length == 3);

            List<string> list = new List<string>();

            foreach (string user in users)
            {
                list.Add(user);
            }

            Assert.That(list.Contains("GetUsersInRoleUser1"));
            Assert.That(list.Contains("GetUsersInRoleUser2"));
            Assert.That(list.Contains("GetUsersInRoleUser3"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is populated.
        /// </summary>
        /// <expectedResult>
        /// A successfull return of all users in the role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestFindUsersInRoleWithPartialMatch()
        {
            string[] users = Roles.FindUsersInRole("RoleForGetUsersInRole", "%3");

            Assert.That(users != null);

            Assert.That(users.Length == 1);

            List<string> list = new List<string>();

            foreach (string user in users)
            {
                list.Add(user);
            }

            Assert.That(list.Contains("GetUsersInRoleUser3"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is populated but specifying a name that doesn't match anything.
        /// </summary>
        /// <expectedResult>
        /// An empty string array is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestFindUsersInRoleWithoutMatch()
        {
            string[] users = Roles.FindUsersInRole("RoleForGetUsersInRole", "SomethingThatWontMatch");

            Assert.That(users != null);

            Assert.That(users.Length == 0);
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of an existing role that is unpopulated.
        /// </summary>
        /// <expectedResult>
        /// An empty string array is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestFindUsersInRoleWhenNoUsersInRole()
        {
            string[] users = Roles.FindUsersInRole(GC_UNPOPULATED_ROLE, "%");

            Assert.That(users != null);

            Assert.That(users.Length == 0);
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of a role that does not exist.
        /// </summary>
        /// <expectedResult>
        /// A provider exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException))]
        [Test]
        public void TestFindUsersInRoleInvalidRole()
        {
            Roles.FindUsersInRole(GC_NON_EXISTENT_ROLE, "%");
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in the name
        /// of an role that has a comma in.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestFindUsersInRoleWithCommaInRole()
        {
            Roles.FindUsersInRole(GC_USER_NAME_WITH_COMMA, "%");
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in null as
        /// the name of the role.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestFindUsersInRoleWithNullRole()
        {
            Roles.FindUsersInRole(null, "%");
        }

        /// <summary>
        /// Tests the calling of the FindUsersInRole method of the Role Provider passing in an empty
        /// string as the name of the role.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        [Test]
        public void TestFindUsersInRoleStringRole()
        {
            Roles.FindUsersInRole(string.Empty, "%");
        }

        #endregion

        #region IsUserInRole Tests

        /// <summary>
        /// Tests the calling of the IsUserInRole method passing in an existing user and a role where
        /// the user exists and is in the specified role.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestIsUserInRoleWithUserInRole()
        {
            bool result = Roles.IsUserInRole("UserInARole", "PopulatedRole");

            Assert.AreEqual(true, result);
        }

        /// <summary>
        /// Tests the calling of the IsUserInRole method passing in an existing user and a role where
        /// the user exists but is not in the specified existing role.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestIsUserInRoleWithUserNotInRole()
        {
            bool result = Roles.IsUserInRole("Username1", "Rolename1");

            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests the calling of the IsUserInRole method passing in an existing user and a 
        /// nonexistent role.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        [Test]
        public void TestIsUserInRoleWithValidUserAndInvalidRole()
        {
            bool result = Roles.IsUserInRole("Username1", GC_NON_EXISTENT_ROLE);

            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests the calling of the IsUserInRole method passing in a nonexisent user and a valid 
        /// role name.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user was not found in the database.")]
        [Test]
        public void TestIsUserInRoleWithInvalidUserAndValidRole()
        {
            bool result = Roles.IsUserInRole(GC_NON_EXISTENT_USER, "Rolename1");

            Assert.AreEqual(false, result);
        }

        /// <summary>
        /// Tests the calling of the IsUserInRole method passing in an nonexistent user and a 
        /// nonexistent role.
        /// </summary>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException))]
        [Test]
        public void TestIsUserInRoleWithInvalidUserAndInvalidRole()
        {
            bool result = Roles.IsUserInRole(GC_NON_EXISTENT_USER, GC_NON_EXISTENT_ROLE);

            Assert.AreEqual(false, result);
        }

        #endregion

        #region Removing Users From Roles Tests

        #region RemoveUserFromRole Tests

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// valid username and a valid rolenames with the user not already in the role.
        /// </summary>
        /// <expectedResult>
        /// The user is added to the role.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRemoveUserFromRole()
        {
            try
            {
                // Validate that the user is in the role
                Assert.IsTrue(Roles.IsUserInRole("UserInARole", "PopulatedRole"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Call the role provider so that the user is added role
            Roles.RemoveUserFromRole("UserInARole", "PopulatedRole");

            try
            {
                // Validate that the user has successfully been added to the role.
                Assert.IsFalse(Roles.IsUserInRole("UserInARole", "PopulatedRole"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }
        
        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// valid role but a username with a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRoleWithCommaInName()
        {
            // Call the role provider
            Roles.RemoveUserFromRole("Userna,me1", "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// null for the username but a valid rolename.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestARemoveUserFromRoleWithNullInName()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole(null, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in an
        /// empty string for the username and a valid rolename.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRoleWithEmptyStringInName()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole(string.Empty, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// null role but a valid username.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUserFromRoleWithNullInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole("Username1", null);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in an
        /// empty string for the rolename and a valid username.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRoleWithEmptyStringInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole("Username1", string.Empty);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// valid role and username but the user is already in the role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'Username1' is already not in role 'Rolename1'.")]
        public void TestRemoveUserFromRoleWithAUserNotInRole()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole("Username1", "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// non-existent user and a valid role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentUser' was not found.")]
        public void TestRemoveUserFromRoleWithUsernameNotExisting()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole(GC_NON_EXISTENT_USER, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRole method of the Ingres Role Provider passing in a 
        /// valid user but a non-existent role.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        public void TestRemoveUserFromRoleWithRolenameNotExisting()
        {
            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRole("Username1", GC_NON_EXISTENT_ROLE);
        }

        #endregion

        #region RemoveUserFromRoles Tests

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRemoveUserFromRoles()
        {
            try
            {
                // Validate that the users are not already in the roles
                Assert.AreEqual(true, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole1"));
                Assert.AreEqual(true, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole2"));
                Assert.AreEqual(true, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] rolenames = new string[] { "MultipleRole1", "MultipleRole2", "MultipleRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("UserInMultipleRoles", rolenames);

            try
            {
                // Validate that the users have successfully been added to the roles.
                Assert.AreEqual(false, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole1"));
                Assert.AreEqual(false, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole2"));
                Assert.AreEqual(false, Roles.IsUserInRole("UserInMultipleRoles", "MultipleRole3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRolesWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "MultipleRole1", "MultipleRole2", "MultipleRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("Usern,ame1", rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUserFromRolesWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] rolenames = new string[] { "MultipleRole1", "MultipleRole2", "MultipleRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles(null, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRolesWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles(string.Empty, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUserFromRolesWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] rolenames = new string[] { "Rolename1", null, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUserFromRolesWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] rolenames = new string[] { "Rolename1", string.Empty, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("Username1", rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'UserInMultipleRoles' is already not in role 'Rolename3'.")]
        public void TestRemoveUserFromRolesWithAUserNotInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "MultipleRole1", "MultipleRole2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("UserInMultipleRoles", rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentUser' was not found.")]
        public void TestRemoveUserFromRolesWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "MultipleRole1", "MultipleRole2", "MultipleRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles(GC_NON_EXISTENT_USER, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUserFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'UserInMultiplesRoles' was not found.")]
        public void TestRemoveUserFromRolesWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] rolenames = new string[] { "InvalidRolename", "MultipleRole2", "MultipleRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUserFromRoles("UserInMultiplesRoles", rolenames);
        }

        #endregion

        #region RemoveUsersFromRoles Tests

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRemoveUsersFromRoles()
        {
            try
            {
                // Validate that the users are in the roles
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole1"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole2"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole3"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole1"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole2"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole3"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole1"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole2"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", "UsersInRolesUser3" };

            string[] rolenames = new string[] { "UsersInRolesRole1", "UsersInRolesRole2", "UsersInRolesRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);

            try
            {
                // Validate that the users have successfully been removed from the roles.
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole1"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole2"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole3"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole1"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole2"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole3"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole1"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole2"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole3"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRolesWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUsersFromRolesWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] usernames = new string[] { "Username1", null, "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRolesWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] usernames = new string[] { "Username1", string.Empty, "Username3" };

            string[] rolenames = new string[] { "Rolename1", "Rolename2", "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUsersFromRolesWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", null, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRolesWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            string[] rolenames = new string[] { "Rolename1", string.Empty, "Rolename3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'Username1' is already not in role 'UsersInRolesRole1'.")]
        public void TestRemoveUsersFromRolesWithAUserNoteInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "UsersInRolesUser1", "UsersInRolesUser2" };

            string[] rolenames = new string[] { "UsersInRolesRole1", "UsersInRolesRole2", "UsersInRolesRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentUser' was not found.")]
        public void TestRemoveUsersFromRolesWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", GC_NON_EXISTENT_USER };

            string[] rolenames = new string[] { "UsersInRolesRole1", "UsersInRolesRole2", "UsersInRolesRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRoles method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        public void TestRemoveUsersFromRolesWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", "UsersInRolesUser3" };

            string[] rolenames = new string[] { GC_NON_EXISTENT_ROLE, "UsersInRolesRole2", "UsersInRolesRole3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRoles(usernames, rolenames);
        }

        #endregion

        #region RemoveUsersFromRole Tests

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of valid usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles.
        /// </summary>
        /// <expectedResult>
        /// All of the specified users are added to all of the specified roles.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestRemoveUsersFromRole()
        {
            try
            {
                // Validate that the users are in the roles
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole1"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole1"));
                Assert.IsTrue(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }

            // Build up string arrays for the users and roles
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", "UsersInRolesUser3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "UsersInRolesRole1");

            try
            {
                // Validate that the users have successfully been added to the roles.
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser1", "UsersInRolesRole1"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser2", "UsersInRolesRole1"));
                Assert.IsFalse(Roles.IsUserInRole("UsersInRolesUser3", "UsersInRolesRole1"));
            }
            catch (Exception)
            {
                throw new Exception(GC_VALIDATION_ERROR);
            }
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it has a comma in it.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRoleWithCommaInName()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUsersFromRoleWithNullInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being null.
            string[] usernames = new string[] { "Username1", null, "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRoleWithEmptyStringInNames()
        {
            // Build up the usernames and rolenames with one of the usernames being an empty string.
            string[] usernames = new string[] { "Username1", string.Empty, "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "Rolename1");
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the rolenames is invalid as
        /// it is null.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        public void TestRemoveUsersFromRoleWithNullInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being null.
            string[] usernames = new string[] { "Username1", "User,name2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, null);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with none of
        /// the given users already in any of the given roles. One of the usernames is invalid as
        /// it is an empty string.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ArgumentException))]
        public void TestRemoveUsersFromRoleWithEmptyStringInRoles()
        {
            // Build up the usernames and rolenames with one of the rolenames being an empty string.
            string[] usernames = new string[] { "Username1", "Username2", "Username3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, string.Empty);
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the users
        /// already in one of the roles.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'Username1' is already not in role 'UsersInRolesRole1'.")]
        public void TestRemoveUsersFromRoleWithAUserNotInARole()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", "Username1" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "UsersInRolesRole1");
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the usernames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The user 'NonExistentRole' was not found.")]
        public void TestRemoveUsersFromRoleWithUsernameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", GC_NON_EXISTENT_ROLE };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, "UsersInRolesRole1");
        }

        /// <summary>
        /// Tests a call to the RemoveUsersFromRole method of the Ingres Role Provider passing in a 
        /// string array of usernames and a string array of valid rolenames with one of the rolenames
        /// not existing.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "The role 'NonExistentRole' was not found.")]
        public void TestRemoveUsersFromRoleWithRolenameNotExisting()
        {
            // Build up the usernames and rolenames with one of the usernames having a comma.
            string[] usernames = new string[] { "UsersInRolesUser1", "UsersInRolesUser2", "UsersInRolesUser3" };

            // Call the role provider so that the usernames are added to the roles
            Roles.RemoveUsersFromRole(usernames, GC_NON_EXISTENT_ROLE);
        }

        #endregion

        #endregion

        #region Miscellaneous Tests

        /// <summary>
        /// Tests that the application name is correctly being retrieved from the configuration
        /// file.
        /// </summary>
        /// <expectedResult>
        /// The application name is retrieved from the configuration file.
        /// </expectedResult>
        [Category(GC_CATEGORY_ROLE_PROVIDER)]
        [Test]
        public void TestApplicationNameRetrieval()
        {
            Assert.AreEqual("IngresRoleProvider Test Fixture", Roles.ApplicationName);
        }

        #endregion

        #endregion

        #region Database Connection and DbUnit Details

        /// <summary>
        /// Set up method for the NUnit test suite.
        /// </summary>
        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            // Retrieve the connection details from the App.config file
            ConnectionStringSettingsCollection connectionSettings = ConfigurationManager.ConnectionStrings;

            if (connectionSettings.Count == 0)
            {
                throw new InvalidOperationException("No connection information specified in application configuration file.");
            }

            // Instantiate a connection to the Ingres database using the settings in the App.config
            // file.
            ConnectionStringSettings connectionSetting = connectionSettings[0];

            string myConnectionString = connectionSetting.ConnectionString;

            this.Connection = new IngresConnection();

            this.Connection.ConnectionString = myConnectionString;

            // Open the Ingres connection
            this.Connection.Open();

            // Setup the primary key filter (for DBUnit)
            PrimaryKeyFilter primaryKeyFilter = new PrimaryKeyFilter();

            string[] usersInRolesKeys = { "UserId", "RoleId" };

            primaryKeyFilter.Add("aspnet_Applications", new PrimaryKey("ApplicationId"));
            primaryKeyFilter.Add("aspnet_Users", new PrimaryKey("UserId"));
            primaryKeyFilter.Add("aspnet_Roles", new PrimaryKey("RoleId"));
            primaryKeyFilter.Add("aspnet_UsersInRoles", new PrimaryKey(usersInRolesKeys));
           
            PKFilter = primaryKeyFilter;

            DatabaseHandler = new GenericDatabaseHandler(this.Connection);
        }

        /// <summary>
        /// Tear down method for the test fixture.
        /// </summary>
        [TestFixtureTearDown]
        public void TearDownFixture()
        {
            // Close the connection if it is not already closed (and exists)
            if ((this.Connection.State != ConnectionState.Closed) && (this.Connection != null))
            {
                this.Connection.Close();
            }
        }

        /// <summary>
        /// Set up method for individual test cases.
        /// </summary>
        [SetUp]
        public void SetUpTestCase()
        {
            // We must validate that the database is empty
            this.ValidateDatabaseIsEmpty();

            // Now call the base DBUnit method to populate the database
            SetUp();
        }

        /// <summary>
        /// Tear down method for individual test cases.
        /// </summary>
        [TearDown]
        public void TearDownTestCase()
        {
            // Call the base DBUnit method which should remove all of the test data
            TearDown();

            // We must validate that the database is empty
            this.ValidateDatabaseIsEmpty();
        }

        /// <summary>
        /// Return the DataSet to be used by DBUnit for these tests.
        /// </summary>
        /// <returns>The Data Set containing the test data.</returns>
        protected override IDataSet GetDataSet()
        {
            return new EmbeddedStructuredXmlDataSet(GetType());
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper method to check that the Ingres database used for testing does not have any
        /// data in the Roles tables prior to commencing set up of a test case and after we
        /// tear down a test case.
        /// </summary>
        private void ValidateDatabaseIsEmpty()
        {
            // Validate that the aspnet_UsersInRoles table is empty
            IngresCommand cmd = new IngresCommand();

            cmd.Connection = this.Connection;

            cmd = new IngresCommand();

            cmd.Connection = this.Connection;

            string sql = @"SELECT
                               COUNT(*)
                           FROM
                               aspnet_UsersInRoles";

            cmd.CommandText = sql;

            int rows = (int)cmd.ExecuteScalar();

            cmd.ExecuteNonQuery();

            if (rows != 0)
            {
                throw new Exception("The aspnet_UsersInRoles table is not empty.");
            }

            // Validate that the aspnet_Users table is empty
            cmd = new IngresCommand();

            cmd.Connection = this.Connection;

            sql = @"SELECT
                        COUNT(*)
                    FROM
                        aspnet_Users";

            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();

            rows = (int)cmd.ExecuteScalar();

            if (rows != 0)
            {
                throw new Exception("The aspnet_Users table is not empty.");
            }

            // Validate that the aspnet_Roles table is empty
            cmd = new IngresCommand();

            cmd.Connection = this.Connection;

            sql = @"SELECT
                        COUNT(*)
                    FROM
                        aspnet_Roles";

            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();

            rows = (int)cmd.ExecuteScalar();

            if (rows != 0)
            {
                throw new Exception("The aspnet_Roles table is not empty.");
            }

            // Validate that the aspnet_Application table is empty.
            sql = @"SELECT
                        COUNT(*)
                    FROM
                        aspnet_Applications";

            cmd.CommandText = sql;

            cmd.ExecuteNonQuery();

            rows = (int)cmd.ExecuteScalar();

            // The aspnet_applications table may contain 0 or 1 rows, depending where we are
            // in the test fixture.
            if (rows != 0)
            {
                throw new Exception("The aspnet_Applications is not empty.");
            }
        }

        #endregion
    }

    #endregion
}
