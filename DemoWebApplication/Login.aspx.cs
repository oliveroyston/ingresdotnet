namespace DemoWebApplication
{
    using System;
    using System.Web.Security;

    /// <summary>
    /// Partial class for the code-behind for the Login page.
    /// </summary>
    public partial class _default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Membership.ValidatingPassword += OnValidatePassword;
        }

        private static void OnValidatePassword(object sender, ValidatePasswordEventArgs args)
        {
            // Cancel if the password is F0rb!dden
            if (args.Password == "F0rb!dden")
            {
                args.Cancel = true;
                args.FailureInformation = new MembershipPasswordException("The password 'F0rb!dden' is not allowed!");
            }

            // Regex validation or any other type of validation could be done here :)
            // An example regex implementation is commented out below:

            // System.Text.RegularExpressions.Regex r =
            //  new System.Text.RegularExpressions.Regex(...);

            // if (!r.IsMatch(args.Password))
            // {
            //     args.FailureInformation =
            //       new Exception(...);
            //     args.Cancel = true;
            // }
        }
    }
}
