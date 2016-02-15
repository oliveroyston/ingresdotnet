<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Main.aspx.cs" Inherits="DemoWebApplication.Authenticated.Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Ingres ASP.NET Role and Membership Providers | Logged In</title>
    <style type="text/css">
        .loggedIn {
            background-color: #FFFFCC;
        }
    </style>
</head>
<body class="loggedIn">
    <form id="form1" runat="server">
    <div>
    <h2>You have successfully Logged In</h2>
    
    <h3>LoginName Control</h3>
    <p>Hello <asp:LoginName ID="LoginName1" runat="server" />! Your username in the previous sentence was provided by the LoginName control.</p>
    
    <h3>LoginStatus Control</h3>
    <p>Here is the LoginStatus control:</p>
    <asp:LoginStatus ID="LoginStatus1" runat="server" 
        LogoutAction="RedirectToLoginPage" />
    <p>As you are currently logged in, clicking it will log you out.</p>
    
    <h3>ChangePassword Control</h3>
    <p>Below is an "out-of-the-box" ASP.NET ChangePassword control. This was dragged and and dropped onto the web form and then styled via the designer.</p>
    <asp:ChangePassword ID="ChangePassword1" runat="server" BackColor="#F7F7DE" 
            BorderColor="#CCCC99" BorderStyle="Solid" BorderWidth="1px" 
            Font-Names="Verdana" Font-Size="10pt" 
            ContinueDestinationPageUrl="~/Authenticated/Main.aspx">
        <TitleTextStyle BackColor="#6B696B" Font-Bold="True" 
            ForeColor="#FFFFFF" />
    </asp:ChangePassword>
    
    <h3>Role Restricted Content</h3>
    <p>If you are in the Administrator role you can navigate <a href="../Admin/Admin.aspx">here</a>. If you are not an Administrator you will be redirected to the login page.</p>
    
    <h3>LoginView Control</h3>
    <asp:LoginView ID="LoginView1" runat="server">
        <LoggedInTemplate>
            <p>This text is displayed to all logged in users. If you were assigned
               to the Administrator or User role you would see different content.
            </p>
        </LoggedInTemplate>
        <RoleGroups>
            <asp:RoleGroup Roles="Administrator">
                <ContentTemplate>
                    <p>As you are an Administrator you can see this content!</p>
                </ContentTemplate>
            </asp:RoleGroup>
            <asp:RoleGroup Roles="User">
                <ContentTemplate>
                    <p>As you are a User you can see this content!</p>
                </ContentTemplate>
            </asp:RoleGroup>
        </RoleGroups>
    </asp:LoginView>
    
    <h3>More...</h3>
    
    <p>Here are the roles in the system:</p>
    
    <!-- Note: This is not best practice. Use codebehinds in real sites! -->
    
    <%
        foreach (string role in Roles.GetAllRoles())
        {
            Response.Write(role);
            Response.Write("<br />");
        }
    %>
    
    <p>Here are some of the details of the current user:</p>
    
    <%MembershipUser user = Membership.GetUser();%>
    
    <strong>Email:</strong> <%= user.Email %><br />
    <strong>Key:</strong> <%= user.ProviderUserKey %><br />
    <strong>Password Question:</strong> <%= user.PasswordQuestion %><br />
    
    </div>
    </form>
</body>
</html>
