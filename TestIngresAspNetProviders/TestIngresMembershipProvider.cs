#region Code Header

/*  
 * Author               : Oliver P. Oyston (Luminary Solutions)
 * 
 * File Name            : TestIngresMembershipProvider.cs
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
    using System.Web.Security;
    using DbUnit.Dataset;
    using DbUnit.Framework;
    using Ingres.Client;
    using NUnit.Framework;

    #endregion

    #region Test Fixture for the Ingres Membership Provider

    /// <summary>
    /// <para>Test fixture for the Ingres ASP.NET Membership Provider.</para>
    /// <para>We are using DBUnit so that the database is automatically populated with known data at the
    /// start of each test and then emptied of the data at the end of each test.</para>
    /// <para>Please note: some of the exceptions are thrown by the ASP.NET Framework BEFORE the Ingres
    /// Membership Provider methods are even called - hence the expected exceptions are the exceptions
    /// from the framework and not the Ingres Role provider. However, as a belt-and-braces check, 
    /// the Ingres Membership Provider should validate all of its input parameters in case it is directly 
    /// instantiated.</para>
    /// <para>As the Ingres Membership Provider works in its own transactional state we have to do clean up
    /// in the finally block of some tests. The ValidateDatabaseIsEmpty() method ensures that 
    /// before we start any test case (and after we have finished tearing down a test case) that 
    /// the database is as we expect.</para>
    /// </summary>
    [TestFixture]
    public class TestIngresMembershipProvider : DbUnitTestCase
    {
        #region Constants

        /// <summary>
        /// String value to use as the category for these tests.
        /// </summary>
        private const string GC_CATEGORY_MEMBERSHIP_PROVIDER = "Ingres ASP.NET Membership Provider Tests";

        /// <summary>
        /// The default password to use for the test users.
        /// </summary>
        private const string GC_DEFAULT_PASSWORD = "Passw0rd!";

        /// <summary>
        /// The default new password to use for the test users.
        /// </summary>
        private const string GC_DEFAULT_NEW_PASSWORD = "NewPassw0rd!";

        /// <summary>
        /// The default password answer to use for the test users.
        /// </summary>
        private const string GC_DEFAULT_PASSWORD_ANSWER = "Answer";

        /// <summary>
        /// The default new password answer to use for the test users.
        /// </summary>
        private const string GC_DEFAULT_NEW_PASSWORD_ANSWER = "NewAnswer";

        /// <summary>
        /// The default new password answer to use for the test users.
        /// </summary>
        private const string GC_DEFAULT_NEW_PASSWORD_QUESTION = "NewQuestion";

        /// <summary>
        /// An incorrect password answer for use in the tests.
        /// </summary>
        private const string GC_INCORRECT_PASSWORD_ANSWER = "IncorrectPasswordAnswer";

        /// <summary>
        /// An incorrect password for use in the tests.
        /// </summary>
        private const string GC_INCORRECT_PASSWORD = "IncorrectPassword";

        /// <summary>
        /// The user to use when a locked out user is required.
        /// </summary>
        private const string GC_LOCKED_USER = "LockedUser";

        /// <summary>
        /// The default user to use in tests (where appropriate).
        /// </summary>
        private const string GC_DEFAULT_TEST_USER = "Username1";

        /// <summary>
        /// The username to use for tests that require a user whose password and password answer
        /// are stored in a hashed format.
        /// </summary>
        private const string GC_HASHED_USER = "HashedUser";

        /// <summary>
        /// The username to use for tests that require a user whose password and password answer
        /// are stored in an encrypted format.
        /// </summary>
        private const string GC_ENCRYPTED_USER = "EncryptedUser";

        /// <summary>
        /// The username to use for tests that require a non-existent user.
        /// </summary>
        private const string GC_NON_EXISTENT_USER = "NonExistentUser";

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

        #region Ingres Membership Provider Tests

        #region ChangePassword Tests

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying the
        /// correct old password and a valid new password.
        /// </summary>
        /// <expectedResult>
        /// The users password is successfully changed.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePassword()
        {
            // Obtain the user and attempt to change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.That(user.ChangePassword(GC_DEFAULT_PASSWORD, GC_DEFAULT_NEW_PASSWORD));

            // Validate the user to ensure that the password has been changed successfully.
            Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_NEW_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying an
        /// incorrect old password.
        /// </summary>
        /// <expectedResult>
        /// The password is not changed.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordIncorrectOldPassword()
        {
            // Obtain the user and attempt tp change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // The password should not have been changed
            Assert.IsFalse(user.ChangePassword(GC_INCORRECT_PASSWORD, GC_DEFAULT_NEW_PASSWORD));

            // Validate the user to ensure that the password has not been changed
            // and that the password is still the old password.
            Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying null
        /// for the old password.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: oldPassword")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordNullOldPassword()
        {
            // Obtain the user and attempt tp change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            user.ChangePassword(null, GC_DEFAULT_NEW_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying an
        /// empty string for the old password.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'oldPassword' must not be empty.\r\nParameter name: oldPassword")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordEmptyStringOldPassword()
        {
            // Obtain the user and attempt tp change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            user.ChangePassword(string.Empty, GC_DEFAULT_NEW_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying null
        /// for the new password.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: newPassword")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordNullNewPassword()
        {
            // Obtain the user and attempt tp change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            user.ChangePassword(GC_DEFAULT_PASSWORD, null);
        }

        /// <summary>
        /// Tests the calling of the ChangePassword method on a MembershipUser and specifying an 
        /// empty string for the new password.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'newPassword' must not be empty.\r\nParameter name: newPassword")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordEmptyStringNewPassword()
        {
            // Obtain the user and attempt tp change their password.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            user.ChangePassword(GC_DEFAULT_PASSWORD, string.Empty);
        }

        #endregion

        #region ChangePasswordQuestionAndAnswer Tests

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying the
        /// correct password and a valid new password question and answer.
        /// </summary>
        /// <expectedResult>
        /// The users password is successfully changed.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswer()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);
            
            // Attempt to change the password question and answer
            Assert.That(user.ChangePasswordQuestionAndAnswer(GC_DEFAULT_PASSWORD, GC_DEFAULT_NEW_PASSWORD_QUESTION, GC_DEFAULT_NEW_PASSWORD_ANSWER));

            user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Ensure that the password question has been successfully set
            Assert.That(user.PasswordQuestion == GC_DEFAULT_NEW_PASSWORD_QUESTION);

            // Ensure that the password answer has been successfully set (by attempting to change
            // the users password with it (throws an error if the passsword answer is incorrect).
            user.ResetPassword(GC_DEFAULT_NEW_PASSWORD_ANSWER);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying the
        /// wrong password.
        /// </summary>
        /// <expectedResult>
        /// The password question and answer are not changed.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerWrongPassword()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            Assert.IsFalse(user.ChangePasswordQuestionAndAnswer(GC_INCORRECT_PASSWORD, GC_DEFAULT_NEW_PASSWORD_QUESTION, GC_DEFAULT_NEW_PASSWORD_ANSWER));
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying
        /// null for the password.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: password")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerNullPassword()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(null, GC_DEFAULT_NEW_PASSWORD_QUESTION, GC_DEFAULT_NEW_PASSWORD_ANSWER);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying
        /// an empty string for the password.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'password' must not be empty.\r\nParameter name: password")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerEmptyStringPassword()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(string.Empty, GC_DEFAULT_NEW_PASSWORD_QUESTION, GC_DEFAULT_NEW_PASSWORD_ANSWER);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying
        /// null for the password question.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: newPasswordQuestion")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerNullPasswordQuestion()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(GC_DEFAULT_PASSWORD, null, GC_DEFAULT_NEW_PASSWORD_ANSWER);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying
        /// an empty string for the password question.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'newPasswordQuestion' must not be empty.\r\nParameter name: newPasswordQuestion")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerEmptyStringPasswordQuestion()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(GC_DEFAULT_PASSWORD, string.Empty, GC_DEFAULT_NEW_PASSWORD_ANSWER);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying 
        /// null for the password answer.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: newPasswordAnswer")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerNullPasswordAnswer()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(GC_DEFAULT_PASSWORD, GC_DEFAULT_NEW_PASSWORD_QUESTION, null);
        }

        /// <summary>
        /// Tests the calling of the ChangePasswordQuestionAndAnswer method on a MembershipUser and specifying
        /// an empty string for the password answer.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'newPasswordAnswer' must not be empty.\r\nParameter name: newPasswordAnswer")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestChangePasswordQuestionAndAnswerEmptyStringPasswordAnswer()
        {
            // Retrieve the user
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            // Attempt to change the password question and answer
            user.ChangePasswordQuestionAndAnswer(GC_DEFAULT_PASSWORD, GC_DEFAULT_NEW_PASSWORD_QUESTION, string.Empty);
        }

        #endregion

        #region CreateUser Tests

        /// <summary>
        /// Tests a successful call to the CreateUser method of the Membership provider.
        /// </summary>
        /// <expectedResult>
        /// A user is successfully created.
        /// </expectedResult>/// 
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestCreateUser()
        {
            // user properties
            string username         = "TestUser";
            string password         = GC_DEFAULT_PASSWORD;
            string email            = "testuser@email.com";
            bool isApproved         = true;
            string passwordQuestion = "Test Password Question";
            string passwordAnswer   = "Test Password Answer";

            MembershipCreateStatus status;

            // Create the user
            MembershipUser user = Membership.CreateUser(
                                                        username, 
                                                        password, 
                                                        email, 
                                                        passwordQuestion, 
                                                        passwordAnswer,
                                                        isApproved, 
                                                        out status);

            // Check that we have had a successful creation status returned
            Assert.That(status == MembershipCreateStatus.Success);

            // Ensure that the user has the correct properties
            Assert.AreEqual("TestUser", user.UserName);
            Assert.AreEqual("testuser@email.com", user.Email);
            Assert.AreEqual("Test Password Question", user.PasswordQuestion);
            Assert.AreEqual(true, user.IsApproved);

            // Validate the password has been successfully set by validating the user
            Assert.That(Membership.ValidateUser("TestUser", GC_DEFAULT_PASSWORD));

            // Validate that the password answer has been successfully set by attempting to
            // reset the password using our password answer. This will throw an exception
            // if the password answer is incorrect.
            user.ResetPassword("Test Password Answer");

            // We need a compensatory transaction to remove the user from our database
            Assert.That(Membership.DeleteUser("TestUser", true));
        }

        #endregion

        #region DeleteUser Tests

        /// <summary>
        /// Tests the calling of the DeleteUser method of the Membership provider passing in a 
        /// username that exists.
        /// </summary>
        /// <expectedResult>
        /// The membership user is successfully deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestDeleteUserByUserName()
        {
            string username = GC_DEFAULT_TEST_USER;

            // Delete the user.
            Membership.DeleteUser(username);

            // Attempt to retrieve the user - the user should be null.
            Assert.That(Membership.GetUser(username) == null);
        }

        /// <summary>
        /// Tests the calling of the DeleteUser method of the Membership provider passing in a 
        /// username that exists and specifying that all related data should not be deleted.
        /// </summary>
        /// <expectedResult>
        /// The membership user is successfully deleted and all related data is left.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestDeleteUserByUserNameDeleteAllRelatedDataFalse()
        {
            // Test that the user exists before we attempt to delete him
            Assert.That(Membership.FindUsersByName(GC_DEFAULT_TEST_USER).Count == 1);

            // Attempt to delete the user
            Assert.That(Membership.DeleteUser(GC_DEFAULT_TEST_USER, false));

            // Check that the user has been deleted.
            Assert.That(Membership.FindUsersByName(GC_DEFAULT_TEST_USER).Count == 0);
        }

        /// <summary>
        /// Tests the calling of the DeleteUser method of the Membership provider passing in a 
        /// username that exists and specifying that all related data should be deleted.
        /// </summary>
        /// <expectedResult>
        /// The membership user and all related data is successfully deleted.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestDeleteUserByUserNameDeleteAllRelatedDataTrue()
        {
            string username = GC_DEFAULT_TEST_USER;

            Membership.DeleteUser(username, true);
        }

        /// <summary>
        /// Tests the calling of the DeleteUser method of the Membership provider passing in a 
        /// username that does not exist.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that the user does not exist.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "User does not exist")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestDeleteUserByUserNameWithUserNameNotExisting()
        {
            string username = GC_NON_EXISTENT_USER;

            Membership.DeleteUser(username, true);
        }

        #endregion

        #region FindUsersByEmail Tests

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email to match that has matching users.
        /// </summary>
        /// <expectedResult>
        /// All users that match are successfully returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmail()
        {
            string email = "FindUsersByEmail";

            MembershipUserCollection coll = Membership.FindUsersByEmail(email);

            Assert.That(coll.Count == 6);

            List<string> emails = new List<string>();

            foreach (MembershipUser user in coll)
            {
                emails.Add(user.Email);
            }

            Assert.That(emails.Contains("FindUsersByEmail1@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail2@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail3@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail@test4.com"));
            Assert.That(emails.Contains("FindUsersByEmail@test5.com"));
            Assert.That(emails.Contains("FindUsersByEmail@test6.com"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email domain to match that has matching users.
        /// </summary>
        /// <expectedResult>
        /// All users that match are successfully returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmailSameDomain()
        {
            string email = "@testdomain.com";

            MembershipUserCollection coll = Membership.FindUsersByEmail(email);

            Assert.That(coll.Count == 3);

            List<string> emails = new List<string>();

            foreach (MembershipUser user in coll)
            {
                emails.Add(user.Email);
            }

            Assert.That(emails.Contains("FindUsersByEmail1@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail2@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail3@testdomain.com"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email to match that has matching users. Paging options are also supplied
        /// </summary>
        /// <expectedResult>
        /// All users that match are successfully returned and are correctly paginated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmailWithPagingOptions()
        {
            string emailToMatch = "FindUsersByEmail";
            int pageIndex = 0;
            int pageSize = 2;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 6);

            Assert.That(coll.Count == 2);

            List<string> emails = new List<string>();

            foreach (MembershipUser user in coll)
            {
                emails.Add(user.Email);
            }

            Assert.That(emails.Contains("FindUsersByEmail1@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail2@testdomain.com"));

            pageIndex = 1;

            coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 6);

            Assert.That(coll.Count == 2);

            emails.Clear();

            foreach (MembershipUser user in coll)
            {
                emails.Add(user.Email);
            }

            Assert.That(emails.Contains("FindUsersByEmail3@testdomain.com"));
            Assert.That(emails.Contains("FindUsersByEmail@test4.com"));

            pageIndex = 2;

            coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 6);

            Assert.That(coll.Count == 2);

            emails.Clear();

            foreach (MembershipUser user in coll)
            {
                emails.Add(user.Email);
            }

            Assert.That(emails.Contains("FindUsersByEmail@test5.com"));
            Assert.That(emails.Contains("FindUsersByEmail@test6.com"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email to match that has matching users. Paging options are also supplied and the
        /// supplied page index is negative.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indication that the page index must be greater or equal to zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageIndex must be greater than or equal to zero.\r\nParameter name: pageIndex")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmailWithPagingOptionsNegativePageIndex()
        {
            string emailToMatch = "FindUsersByEmail";
            int pageIndex = -1;
            int pageSize = 2;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email to match that has matching users. Paging options are also supplied and the
        /// supplied page size is less than one.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indication that the page size must be greater than zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageSize must be greater than zero.\r\nParameter name: pageSize")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmailWithPagingOptionsPageSizeLessThanOne()
        {
            string emailToMatch = "FindUsersByEmail";
            int pageIndex = 1;
            int pageSize = 0;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Tests the calling of the FindUsersByEmail method of the Membership provider passing in
        /// an email to match that has matching users. Paging options are also supplied and the
        /// supplied page size is negative.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indication that the page size must be greater than zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageSize must be greater than zero.\r\nParameter name: pageSize")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByEmailWithPagingOptionsPageSizeNegative()
        {
            string emailToMatch = "FindUsersByEmail";
            int pageIndex = 1;
            int pageSize = -1;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByEmail(emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        #endregion

        #region FindUsersByName Tests

        /// <summary>
        /// Tests the calling of the FindUsersByName method of the Membership provider passing in the name of a 
        /// username that has matches.
        /// </summary>
        /// <expectedResult>
        /// All matching users are successfully returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByName()
        {
            string username = "FindUsersByName";

            MembershipUserCollection coll = Membership.FindUsersByName(username);

            Assert.That(coll.Count == 6);

            List<string> usernames = new List<string>();

            foreach (MembershipUser user in coll)
            {
                usernames.Add(user.UserName);
            }

            Assert.That(usernames.Contains("FindUsersByName1"));
            Assert.That(usernames.Contains("FindUsersByName2"));
            Assert.That(usernames.Contains("FindUsersByName3"));
            Assert.That(usernames.Contains("FindUsersByName4"));
            Assert.That(usernames.Contains("FindUsersByName5"));
            Assert.That(usernames.Contains("FindUsersByName1_2"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersByName method of the Membership provider passing in the name of a 
        /// username that exists and specifying paging options.
        /// </summary>
        /// <expectedResult>
        /// All matching users are successfully returned and are correctly paginated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByNameWithPagingOptions()
        {
            string username = "FindUsersByName";
            int pageIndex   = 0;
            int pageSize    = 3;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 6);

            Assert.That(coll.Count == 3);

            List<string> usernames = new List<string>();

            foreach (MembershipUser user in coll)
            {
                usernames.Add(user.UserName);
            }

            Assert.That(usernames.Contains("FindUsersByName1"));
            Assert.That(usernames.Contains("FindUsersByName1_2"));
            Assert.That(usernames.Contains("FindUsersByName2"));

            pageIndex = 1;

            coll = Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 6);

            Assert.That(coll.Count == 3);

            usernames.Clear();

            foreach (MembershipUser user in coll)
            {
                usernames.Add(user.UserName);
            }

            Assert.That(usernames.Contains("FindUsersByName3"));
            Assert.That(usernames.Contains("FindUsersByName4"));
            Assert.That(usernames.Contains("FindUsersByName5"));
        }

        /// <summary>
        /// Tests the calling of the FindUsersByName method of the Membership provider passing in the name of a 
        /// username that exists and specifying paging options. The supplied page index is negative.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that the page index must be greater than are equal to zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageIndex must be greater than or equal to zero.\r\nParameter name: pageIndex")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByNameWithPagingOptionsNegativePageIndex()
        {
            string username = "FindUsersByName";
            int pageIndex = -1;
            int pageSize = 3;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Tests the calling of the FindUsersByName method of the Membership provider passing in the name of a 
        /// username that exists and specifying paging options. The supplied page size is zero.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that the page size must be greater than zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageSize must be greater than zero.\r\nParameter name: pageSize")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByNameWithPagingOptionsPageSizeLessThanOne()
        {
            string username = "FindUsersByName";
            int pageIndex = 0;
            int pageSize = 0;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords);
        }

        /// <summary>
        /// Tests the calling of the FindUsersByName method of the Membership provider passing in the name of a 
        /// username that exists and specifying paging options. The supplied page size is negative.
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that the page size must be greater than zero.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The pageSize must be greater than zero.\r\nParameter name: pageSize")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestFindUsersByNameWithPagingOptionsPageSizeNegative()
        {
            string username = "FindUsersByName";
            int pageIndex = 0;
            int pageSize = -1;
            int totalRecords;

            MembershipUserCollection coll = Membership.FindUsersByName(username, pageIndex, pageSize, out totalRecords);
        }

        #endregion

        #region GetAllUsers Tests

        /// <summary>
        /// Tests the calling of the GetAllUsers method of the Membership provider.
        /// </summary>
        /// <expectedResult>
        /// All members are successfully retrieved.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetAllUsers()
        {
            MembershipUserCollection coll = Membership.GetAllUsers();

            Assert.That(coll.Count == 16);

            List<string> userNamesList = new List<string>();

            // Lets just check that the collection contains users with the provider usernames
            // that we expect
            foreach (MembershipUser user in coll)
            {
                userNamesList.Add(user.UserName);
            }

            Assert.That(userNamesList.Contains("Username1"));
            Assert.That(userNamesList.Contains("FindUsersByName1"));
            Assert.That(userNamesList.Contains("FindUsersByName2"));
            Assert.That(userNamesList.Contains("FindUsersByName3"));
            Assert.That(userNamesList.Contains("FindUsersByName4"));
            Assert.That(userNamesList.Contains("FindUsersByName5"));
            Assert.That(userNamesList.Contains("FindUsersByName1_2"));
            Assert.That(userNamesList.Contains("FindUsersByEmail1"));
            Assert.That(userNamesList.Contains("FindUsersByEmail2"));
            Assert.That(userNamesList.Contains("FindUsersByEmail3"));
            Assert.That(userNamesList.Contains("FindUsersByEmail4"));
            Assert.That(userNamesList.Contains("FindUsersByEmail5"));
            Assert.That(userNamesList.Contains("FindUsersByEmail6"));
            Assert.That(userNamesList.Contains("LockedUser"));
            Assert.That(userNamesList.Contains("EncryptedUser"));
            Assert.That(userNamesList.Contains("HashedUser"));
        }

        /// <summary>
        /// Tests the calling of the GetAllUsers method of the Membership provider and also providing
        /// paging options.
        /// </summary>
        /// <expectedResult>
        /// All expected members are successfully retrieved.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetAllUsersWithPagingOptions()
        {
            int pageIndex = 0;
            int pageSize = 4;
            int totalRecords;

            MembershipUserCollection coll = Membership.GetAllUsers(pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 16);
            Assert.That(coll.Count == 4);

            List<string> userNamesList = new List<string>();

            // Lets just check that the collection contains users with the provider usernames
            // that we expect
            foreach (MembershipUser user in coll)
            {
                userNamesList.Add(user.UserName);
            }

            Assert.That(userNamesList.Contains("EncryptedUser"));
            Assert.That(userNamesList.Contains("FindUsersByEmail1"));
            Assert.That(userNamesList.Contains("FindUsersByEmail2"));
            Assert.That(userNamesList.Contains("FindUsersByEmail3"));

            pageIndex = 1;

            coll = Membership.GetAllUsers(pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 16);
            Assert.That(coll.Count == 4);

            userNamesList.Clear();

            // Lets just check that the collection contains users with the provider usernames
            // that we expect
            foreach (MembershipUser user in coll)
            {
                userNamesList.Add(user.UserName);
            }

            Assert.That(userNamesList.Contains("FindUsersByEmail4"));
            Assert.That(userNamesList.Contains("FindUsersByEmail5"));
            Assert.That(userNamesList.Contains("FindUsersByEmail6"));
            Assert.That(userNamesList.Contains("FindUsersByName1"));

            pageIndex = 2;

            coll = Membership.GetAllUsers(pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 16);
            Assert.That(coll.Count == 4);

            userNamesList.Clear();

            // Lets just check that the collection contains users with the provider usernames
            // that we expect
            foreach (MembershipUser user in coll)
            {
                userNamesList.Add(user.UserName);
            }

            Assert.That(userNamesList.Contains("FindUsersByName1_2"));
            Assert.That(userNamesList.Contains("FindUsersByName2"));
            Assert.That(userNamesList.Contains("FindUsersByName3"));
            Assert.That(userNamesList.Contains("FindUsersByName4"));

            pageIndex = 3;

            coll = Membership.GetAllUsers(pageIndex, pageSize, out totalRecords);

            Assert.That(totalRecords == 16);
            Assert.That(coll.Count == 4);

            userNamesList.Clear();

            // Lets just check that the collection contains users with the provider usernames
            // that we expect
            foreach (MembershipUser user in coll)
            {
                userNamesList.Add(user.UserName);
            }

            Assert.That(userNamesList.Contains("FindUsersByName5"));
            Assert.That(userNamesList.Contains("HashedUser"));
            Assert.That(userNamesList.Contains("LockedUser"));
            Assert.That(userNamesList.Contains("Username1"));            
        }

        #endregion

        #region GetNumberOfUsersOnline Tests

        /// <summary>
        /// Tests the calling of the GetNumberOfUsersOnline method of the Membership provider.
        /// </summary>
        /// <expectedResult>
        /// The number of users online is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetNumberOfUsersOnlineNoUsersOnline()
        {
            // The test data is set up so that no users have been online recently.
            Assert.That(Membership.GetNumberOfUsersOnline() == 0);
        }

        /// <summary>
        /// Tests the calling of the GetNumberOfUsersOnline method of the Membership provider with
        /// one user that is currently online.
        /// </summary>
        /// <expectedResult>
        /// The correct number of online users is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetNumberOfUsersOnlineOneUserOnline()
        {
            // Log a user in so that his last activity dtm is updated.
            Membership.GetUser(GC_DEFAULT_TEST_USER, true);

            // Check how many users are online
            Assert.That(Membership.GetNumberOfUsersOnline() == 1);
        }

        #endregion

        #region GetPassword Tests

        /// <summary>
        /// Tests the calling of the GetPassword method of the Membership provider and no supplying
        /// a password answer.
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indicating that an incorrect password answer has been supplied.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(MembershipPasswordException), ExpectedMessage = "Incorrect password answer.")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetPasswordNoAnswerSpecified()
        {
            // Retrieve a users password and check that it is as expected.
            Assert.That(Membership.GetUser(GC_DEFAULT_TEST_USER, true).GetPassword() == GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the GetPassword method of the Membership provider and supplying
        /// the correct password answer (clear password user).
        /// </summary>
        /// <expectedResult>
        /// The correct password is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetPasswordClearPassword()
        {
            // Retrieve a users password and check that it is as expected.
            Assert.That(Membership.GetUser(GC_DEFAULT_TEST_USER, true).GetPassword(GC_DEFAULT_PASSWORD_ANSWER) == GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the GetPassword method of the Membership provider and supplying
        /// the correct password answer (encrypted password user).
        /// </summary>
        /// <expectedResult>
        /// The correct password is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetPasswordEncryptedPassword()
        {
            // Retrieve a users password and check that it is as expected.
            Assert.That(Membership.GetUser(GC_ENCRYPTED_USER, true).GetPassword(GC_DEFAULT_PASSWORD_ANSWER) == GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the GetPassword method of the Membership provider and supplying
        /// the correct password answer (hashed password user).
        /// </summary>
        /// <expectedResult>
        /// An exception is thrown indicating that we cannot retrieve hashed passwords.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "Cannot unencode a hashed password.")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetPasswordHashed()
        {
            // Retrieve a users password and check that it is as expected.
            Assert.That(Membership.GetUser(GC_HASHED_USER, true).GetPassword(GC_DEFAULT_PASSWORD_ANSWER) == GC_DEFAULT_PASSWORD);
        } 

        #endregion

        #region GetUser Tests

        #region GetUser By UserName Tests

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in a username.
        /// </summary>
        /// <expectedResult>
        /// The successful retrieval of a memebership user.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserByUserName()
        {
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.AreEqual("Test User", user.Comment);
            Assert.AreEqual(DateTime.Parse("2008-05-10"), user.CreationDate);
            Assert.AreEqual("TestUser@Test.com", user.Email);
            Assert.AreEqual(true, user.IsApproved);
            Assert.AreEqual(false, user.IsLockedOut);
            Assert.AreEqual(false, user.IsOnline);
            Assert.AreEqual(DateTime.Parse("2008-01-01"), user.LastActivityDate);
            Assert.AreEqual(DateTime.Parse("2008-02-02"), user.LastLockoutDate);
            Assert.AreEqual(DateTime.Parse("2008-03-03"), user.LastLoginDate);
            Assert.AreEqual(DateTime.Parse("2008-04-04"), user.LastPasswordChangedDate);
            Assert.AreEqual("Question", user.PasswordQuestion);
            Assert.AreEqual("IngresMembershipProvider", user.ProviderName);
            Assert.AreEqual("771773c2-bc5b-46cc-a83a-7a94c4e7fc10", user.ProviderUserKey);
            Assert.AreEqual(GC_DEFAULT_TEST_USER, user.UserName);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in a username
        /// that does not exist.
        /// </summary>
        /// <expectedResult>
        /// The returned user is null.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserByUserNameNonExistentUsername()
        {
            MembershipUser user = Membership.GetUser(GC_NON_EXISTENT_USER);

            Assert.That(user == null);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in null for
        /// the username.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestGetUserByUserNameNullUsername()
        {
            MembershipUser user = Membership.GetUser(null);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in an
        /// empty string for the username.
        /// </summary>
        /// <expectedResult>
        /// A Provider exception is thrown indicating that a username must be specified. 
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "A username must be supplied to get a User.")]
        [Test]
        public void TestGetUserByUserNameEmptyStringUsername()
        {
            MembershipUser user = Membership.GetUser(string.Empty);
        }

        #endregion

        #region GetUser By ProviderKey Tests

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in a Provider User key.
        /// </summary>
        /// <expectedResult>
        /// The successful retrieval of a memebership user.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserByProviderUserKey()
        {
            Guid providerUserKey = new Guid("771773c2-bc5b-46cc-a83a-7a94c4e7fc10");

            MembershipUser user = Membership.GetUser(providerUserKey);

            Assert.AreEqual("Test User", user.Comment);
            Assert.AreEqual(DateTime.Parse("2008-05-10"), user.CreationDate);
            Assert.AreEqual("TestUser@Test.com", user.Email);
            Assert.AreEqual(true, user.IsApproved);
            Assert.AreEqual(false, user.IsLockedOut);
            Assert.AreEqual(false, user.IsOnline);
            Assert.AreEqual(DateTime.Parse("2008-01-01"), user.LastActivityDate);
            Assert.AreEqual(DateTime.Parse("2008-02-02"), user.LastLockoutDate);
            Assert.AreEqual(DateTime.Parse("2008-03-03"), user.LastLoginDate);
            Assert.AreEqual(DateTime.Parse("2008-04-04"), user.LastPasswordChangedDate);
            Assert.AreEqual("Question", user.PasswordQuestion);
            Assert.AreEqual("IngresMembershipProvider", user.ProviderName);
            Assert.AreEqual("771773c2-bc5b-46cc-a83a-7a94c4e7fc10", user.ProviderUserKey);
            Assert.AreEqual(GC_DEFAULT_TEST_USER, user.UserName);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in a username
        /// that does not exist.
        /// </summary>
        /// <expectedResult>
        /// The returned user is null.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserByProviderUserKeyNonExistentUsername()
        {
            Guid providerUserKey = new Guid("99999999-9999-9999-9999-999999999999");

            MembershipUser user = Membership.GetUser(providerUserKey);

            Assert.That(user == null);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in null for
        /// the username.
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ArgumentNullException))]
        [Test]
        public void TestGetUserByProviderUserKeyNullUsername()
        {
            MembershipUser user = Membership.GetUser(null);
        }

        /// <summary>
        /// Tests the calling of the GetUser method of the Membership provider passing in an
        /// empty string for the username.
        /// </summary>
        /// <expectedResult>
        /// A Provider exception is thrown indicating that a username must be specified. 
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [ExpectedException(ExceptionType = typeof(ProviderException), ExpectedMessage = "A username must be supplied to get a User.")]
        [Test]
        public void TestGetUserByProviderUserKeyEmptyStringUsername()
        {
            MembershipUser user = Membership.GetUser(string.Empty);
        }

        #endregion

        #endregion

        #region GetUserNameByEmail Tests

        /// <summary>
        /// Tests a call to the GetUserNameByEmail method of the Membership provider passing in
        /// an existing email.
        /// </summary>
        /// <expectedResult>
        /// A successful retrieval of the user name.
        /// </expectedResult>/// 
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserNameByEmail()
        {
            Assert.That(Membership.GetUserNameByEmail("FindUsersByEmail1@testdomain.com") == "FindUsersByEmail1");
        }

        /// <summary>
        /// Tests a call to the GetUserNameByEmail method of the Membership provider passing in
        /// an existing email (in lowercase).
        /// </summary>
        /// <expectedResult>
        /// A successful retrieval of the user name.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserNameByEmailLoweredUsername()
        {
            Assert.That(Membership.GetUserNameByEmail("FindUsersByEmail1@testdomain.com".ToLower()) == "FindUsersByEmail1");
        }

        /// <summary>
        /// Tests a call to the GetUserNameByEmail method of the Membership provider passing in
        /// a non-existent email (i.e. no user has the email).
        /// </summary>
        /// <expectedResult>
        /// An empty string is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserNameByEmailWithNoMatchingUser()
        {
            Assert.That(Membership.GetUserNameByEmail("someEmailWith@NoMatches.com") == string.Empty);
        }

        /// <summary>
        /// Tests a call to the GetUserNameByEmail method of the Membership provider passing in
        /// null for the email.
        /// </summary>
        /// <expectedResult>
        /// An empty string is returned.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestGetUserNameByEmailNullEmail()
        {
            Assert.That(Membership.GetUserNameByEmail(null) == string.Empty);
        }

        #endregion

        #region ResetPassword Tests

        /// <summary>
        /// Tests the calling of the ResetPassword method of the MembershipProvider passing in the
        /// correct password answer.
        /// </summary>
        /// <expectedResult>
        /// The users password is successfully reset.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestResetPassword()
        {
            Assert.That(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD));
            
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            string newPassword = user.ResetPassword(GC_DEFAULT_PASSWORD_ANSWER);

            Assert.That(Membership.ValidateUser(GC_DEFAULT_TEST_USER, newPassword));
        }

        #endregion

        #region UnlockUser Tests

        /// <summary>
        /// Tests the calling of the UnlockUser method of the Membership provider for a locked
        /// user.
        /// </summary>
        /// <expectedResult>
        /// The user is unlocked.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUnLockUser()
        {
            MembershipUser user = Membership.GetUser(GC_LOCKED_USER);

            Assert.IsTrue(user.IsLockedOut);

            Assert.That(user.UnlockUser());

            user = Membership.GetUser(GC_LOCKED_USER);

            Assert.IsFalse(user.IsLockedOut);
        }

        /// <summary>
        /// Tests the calling of the UnlockUser method of the Membership provider method for
        /// a user that is already unlocked.
        /// </summary>
        /// <expectedResult>
        /// The user remains unlocked. No exception is thrown.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUnLockUserUserAlreadyUnlocked()
        {
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.IsFalse(user.IsLockedOut);

            Assert.That(user.UnlockUser());

            Assert.IsFalse(user.IsLockedOut);
        }

        #endregion

        #region UpdateUser Tests

        /// <summary>
        /// Tests the calling of the UpdateUser of the Membership provider passing in valid values
        /// for all mandatory parameters.
        /// </summary>
        /// <expectedResult>
        /// The user is successfully updated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUpdateUser()
        {           
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            user.Comment          = "New Test Comment";
            user.Email            = "NewTestEmail@Email.com";
            user.IsApproved       = false;
            user.LastActivityDate = DateTime.Parse("02/02/2002");
            user.LastLoginDate    = DateTime.Parse("03/03/2003");

            Membership.UpdateUser(user);
            
            // Validate that the user has successfully been updated.
            user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.AreEqual("New Test Comment", user.Comment);
            Assert.AreEqual("NewTestEmail@Email.com", user.Email);
            Assert.AreEqual(false, user.IsApproved);
            Assert.AreEqual(DateTime.Parse("02/02/2002"), user.LastActivityDate);
            Assert.AreEqual(DateTime.Parse("03/03/2003"), user.LastLoginDate);
        }

        #endregion

        #region ValidateUser Tests

        #region Clear Password

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the clear format. The correct password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is successfull validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserClearPassword()
        {
            bool validated = Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD);

            Assert.AreEqual(true, validated);
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a locked out
        /// user whose password is stored in the clear format. The correct password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserClearPasswordUserLockedOut()
        {
            // The user should validate orgiginally
            bool validated = Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD);
            Assert.AreEqual(true, validated);

            // The user should not be locked out.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.IsFalse(user.IsLockedOut);

            // Lock the user out by
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.That(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // The user should not be validated
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD));
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the clear format. An incorrect password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserWrongPasswordClearPassword()
        {
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a 
        /// non-existent user whose password is stored in the clear format.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserNonExistentUserClearPassword()
        {
            Membership.ValidateUser(GC_NON_EXISTENT_USER, GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider and supplying
        /// null for the username (clear password user).
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown indication that the username cannot be null.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: username")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserNullForUserClearPassword()
        {
            Membership.ValidateUser(null, GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider and supplying
        /// an empty string for the username (clear password user).
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indication that the username cannot be empty.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'username' must not be empty.\r\nParameter name: username")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserEmptyStringUserClearPassword()
        {
            Membership.ValidateUser(string.Empty, GC_DEFAULT_PASSWORD);
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider and supplying
        /// null for the password (clear password user).
        /// </summary>
        /// <expectedResult>
        /// An argument null exception is thrown indication that the password cannot be null.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentNullException), ExpectedMessage = "Value cannot be null.\r\nParameter name: password")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserNullForPasswordClearPassword()
        {
            Membership.ValidateUser(GC_DEFAULT_TEST_USER, null);
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider and supplying
        /// an empty string for the username (clear password user).
        /// </summary>
        /// <expectedResult>
        /// An argument exception is thrown indication that the password cannot be empty.
        /// </expectedResult>
        [ExpectedException(ExceptionType = typeof(ArgumentException), ExpectedMessage = "The parameter 'password' must not be empty.\r\nParameter name: password")]
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserEmptyStringForPasswordClearPassword()
        {
            Membership.ValidateUser(GC_DEFAULT_TEST_USER, string.Empty);
        }
        
        #endregion

        #region Hashed Password

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the hashed format. The correct password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is successfull validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserHashedPassword()
        {
            // Attempt to validate the hashed user.
            Assert.IsTrue(Membership.ValidateUser(GC_HASHED_USER, GC_DEFAULT_PASSWORD));
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a locked out
        /// user whose password is stored in the hashed format. The correct password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserWrongPasswordHashedPassword()
        {
            // Attempt to validate the hashed user specifying an incorrect password.
            Assert.IsFalse(Membership.ValidateUser(GC_HASHED_USER, GC_INCORRECT_PASSWORD));
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the hashed format. An incorrect password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserNonExistentUserHashedPassword()
        {
            Assert.IsFalse(Membership.ValidateUser(GC_NON_EXISTENT_USER, GC_DEFAULT_PASSWORD));
        }

        #endregion

        #region Encrypted Password

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the encrypted format. The correct password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is successfull validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserEncryptedPassword()
        {
            // Attempt to validate the hashed user.
            Assert.IsTrue(Membership.ValidateUser(GC_ENCRYPTED_USER, GC_DEFAULT_PASSWORD));
        }

        /// <summary>
        /// Tests the calling of the ValidateUser method of the Membership provider for a user
        /// whose password is stored in the encrypted format. An incorrect password is supplied.
        /// </summary>
        /// <expectedResult>
        /// The user is not validated.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestValidateUserWrongPasswordEncryptedPassword()
        {
            // Attempt to validate the hashed user specifying an incorrect password.
            Assert.IsFalse(Membership.ValidateUser(GC_ENCRYPTED_USER, GC_INCORRECT_PASSWORD));
        }

        #endregion

        #endregion

        #region Test Locking Out User

        /// <summary>
        /// Tests that a user is locked out if they supply the wrong password more than the allowed
        /// number of times.
        /// </summary>
        /// <expectedResult>
        /// The user is locked out.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUserLockedOutForWrongPassword()
        {
            // The user should not be locked out.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.IsFalse(user.IsLockedOut);
            
            // 1st Incorrect Attempt
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // 2nd Incorrect Attempt
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // Max is set to two so next incorrect attempt should lock the user
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.That(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);
        }

        /// <summary>
        /// Tests that a users incorrect password attempt count is reset if they supply the correct
        /// password answer before they are locked out.
        /// </summary>
        /// <expectedResult>
        /// The users password count is reset .
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUserLockedOutForWrongPasswordCorrectAnswerAtLastAttemptThenAnIncorrect()
        {
            // The user should not be locked out.
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.IsFalse(user.IsLockedOut);

            // 1st Incorrect Attempt
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // 2nd Incorrect Attempt
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // Correct attempt - user validated and not locked out
            Assert.That(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_DEFAULT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);

            // Another incorrect attempt should not lock them out as the count should have been reset
            Assert.IsFalse(Membership.ValidateUser(GC_DEFAULT_TEST_USER, GC_INCORRECT_PASSWORD));
            Assert.IsFalse(Membership.GetUser(GC_DEFAULT_TEST_USER).IsLockedOut);
        }

        /// <summary>
        /// Tests that a user is locked out if they supply the wrong password answer more than the
        /// allowed number of times.
        /// </summary>
        /// <expectedResult>
        /// The user is locked out
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestUserLockedOutForWrongPasswordAnswer()
        {
            MembershipUser user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.IsFalse(user.IsLockedOut);

            // Lock out the user by incorrectly supplying the wrong password
            // answer multiple times.
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    user.ResetPassword(GC_INCORRECT_PASSWORD_ANSWER);
                }
                catch (MembershipPasswordException)
                {
                    // Supress the error;
                }
            }
            
            // Check that the user is locked out.
            user = Membership.GetUser(GC_DEFAULT_TEST_USER);

            Assert.That(user.IsLockedOut);
        }

        #endregion

        #region Miscellaneous Tests

        /// <summary>
        /// Tests that the Membership provider was correctly initialised.
        /// </summary>
        /// <expectedResult>
        /// All values are as expected - i.e. as specified in the config files.
        /// </expectedResult>
        [Category(GC_CATEGORY_MEMBERSHIP_PROVIDER)]
        [Test]
        public void TestMembershipProviderInitialisation()
        {
            // Ensure that the Membership provider has initialised correctly and retrieved the values 
            // from the configuration file.
            Assert.AreEqual("IngresMembershipProvider Test Fixture", Membership.ApplicationName);
            Assert.AreEqual(true, Membership.EnablePasswordReset);
            Assert.AreEqual(true, Membership.EnablePasswordRetrieval);

            MembershipProvider provider = Membership.Provider;

            Assert.AreEqual("IngresMembershipProvider Test Fixture", provider.ApplicationName);
            Assert.AreEqual("Ingres ASP.NET Membership Provider", provider.Description);
            Assert.AreEqual(true, provider.EnablePasswordReset);
            Assert.AreEqual(true, provider.EnablePasswordRetrieval);
            Assert.AreEqual(2, provider.MaxInvalidPasswordAttempts);
            Assert.AreEqual(1, provider.MinRequiredNonAlphanumericCharacters);
            Assert.AreEqual(7, provider.MinRequiredPasswordLength);
            Assert.AreEqual("IngresMembershipProvider", provider.Name);
            Assert.AreEqual(10, provider.PasswordAttemptWindow);
            Assert.AreEqual(MembershipPasswordFormat.Hashed, provider.PasswordFormat);
            Assert.AreEqual(string.Empty, provider.PasswordStrengthRegularExpression);
            Assert.AreEqual(true, provider.RequiresQuestionAndAnswer);
            Assert.AreEqual(true, provider.RequiresUniqueEmail);
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
            primaryKeyFilter.Add("aspnet_Membership",   new PrimaryKey("UserId"));
            primaryKeyFilter.Add("aspnet_Users",        new PrimaryKey("UserId"));
            primaryKeyFilter.Add("aspnet_Roles",        new PrimaryKey("RoleId"));
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
