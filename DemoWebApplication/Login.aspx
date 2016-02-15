<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DemoWebApplication._default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Ingres ASP.NET Role and Membership Providers | Login</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h2>Ingres ASP.NET Role and Membership Providers</h2>
        <h2>Demo Web Application</h2>
        
        <h3>Introduction</h3>
        
        <p>Welcome to the demo website for the Ingres ASP.NET Role and Membership Providers!</p>
        
        <p>This demo website is intended to be a simple example of how to configure the Ingres
           ASP.NET providers for use in a website. The site has been kept to the bare minimum
           and is intended to illustrate the ASP.NET controls rather than good web design practices.
           This demo website also illustrates how to expose the providers as web services.</p>
        
        <p>Before attempting to use the site ensure that the SetupDemoWebApplication
           script has been run, the connection string in the web.config file is correct and
           that the Ingres service running.</p>
        
        <p>The Authenticated folder will only allow access to authenticated users. The Admin
           folder has been set up to only allow users with the Administrator role to view the
           contents.</p>
        
        <h3>Users</h3>
        
        <p>The setup scripts create two users:</p>
        <p>
            Admin - Passw0rd! (Administrator role)<br />
            Oliver - letmein (User role)
        </p>
        
        <p>More users can be created using the signup page or via the ASP.NET Website Administration
           Tool</p>
        
        <h3>The ASP.NET Login Control</h3>
        <p>Below is an &quot;out-of-the-box&quot; ASP.NET Login control. You can log onto the website 
           using the users created by&nbsp; the scripts or create your own users using the 
           signup control or the ASP.NET Website Administration Tool.</p>
        
        <asp:Login ID="Login1" runat="server" BackColor="#F7F7DE" BorderColor="#CCCC99" 
            BorderStyle="Solid" BorderWidth="1px" 
            DestinationPageUrl="~/Authenticated/Main.aspx" Font-Names="Verdana" 
            Font-Size="10pt">
            <TitleTextStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" />
        </asp:Login>
        
        <h3>The ASP.NET PasswordRecovery Control</h3>
        <p>Below is an &quot;out-of-the-box&quot; ASP.NET PasswordRecovery control. Only encrypted or 
           clear passwords may be retrieved. Furthermore, password retrieval must be 
           enabled in the web.config.</p>
        
        <p>For the purposes of the demo website, the password email will be placed in a
           pickup directory. This directory is <code>c:\temp</code> so please make sure
           that the directory exists.</p>
           
        <asp:PasswordRecovery ID="PasswordRecovery1" runat="server" BackColor="#F7F7DE" 
            BorderColor="#CCCC99" BorderStyle="Solid" BorderWidth="1px" 
            Font-Names="Verdana" Font-Size="10pt">
            <TitleTextStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" />
        </asp:PasswordRecovery>
        
        <h3>The ASP.NET CreateUserWizard Control</h3>
        
        <p>Below is an &quot;out-of-the-box&quot; ASP.NET CreateUserWizard control. You may create 
           new users using the form. New users will initially not have any assigned roles.</p>
        
        <asp:CreateUserWizard ID="CreateUserWizard1" runat="server" BackColor="#F7F7DE" 
            BorderColor="#CCCC99" BorderStyle="Solid" BorderWidth="1px" 
            Font-Names="Verdana" Font-Size="10pt" style="margin-right: 0px" 
            CancelDestinationPageUrl="~/Login.aspx" 
            ContinueDestinationPageUrl="~/Login.aspx">
            <SideBarStyle BackColor="#7C6F57" BorderWidth="0px" Font-Size="0.9em" 
                VerticalAlign="Top" />
            <SideBarButtonStyle BorderWidth="0px" Font-Names="Verdana" 
                ForeColor="#FFFFFF" />
            <ContinueButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" 
                BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" 
                ForeColor="#284775" />
            <NavigationButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" 
                BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" 
                ForeColor="#284775" />
            <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" 
                HorizontalAlign="Center" />
            <CreateUserButtonStyle BackColor="#FFFBFF" BorderColor="#CCCCCC" 
                BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" 
                ForeColor="#284775" />
            <TitleTextStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" />
            <StepStyle BorderWidth="0px" />
            <WizardSteps>
                <asp:CreateUserWizardStep ID="CreateUserWizardStep1" runat="server">
                </asp:CreateUserWizardStep>
                <asp:CompleteWizardStep ID="CompleteWizardStep1" runat="server">
                </asp:CompleteWizardStep>
            </WizardSteps>
        </asp:CreateUserWizard>
        
        <h3>Exposing the Role and Membership Providers as Web Services</h3>
        <p>Here are examples of how the Ingres ASP.NET Role and Membership providers may be
           exposed as web services.</p>
        
        <p><a href="MembershipProviderWS.asmx">Membership Web Service</a></p>
        <p><a href="RoleProviderWS.asmx">Role Web Service</a></p>
        <p>Please note: the test form is only available for methods with primitive types as parameters.
           Consequently some Role and Membership methods are not able to be called using the web services
           form.</p>
        <p>See <a href="http://www.codeproject.com/KB/aspnet/WSSecurityProvider.aspx">here</a> for
        more information about implementing Role and Membership providers as Web Services.</p>        
    </div>
    </form>
</body>
</html>
