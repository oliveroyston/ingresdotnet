<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="DemoWebApplication.Admin.Admin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Ingres ASP.NET Role and Membership Providers | Admin</title>
    <style type="text/css">
        .admin {
            background-color: #CCFFFF;
        }
    </style>
</head>
<body class="admin">
    <form id="form1" runat="server">
    <div>
    <h2>Administrators Only</h2>
    <p>This page is only viewable if the user is in the Administrators role.</p>
    <p>If the user was not in the Administrator role then they would have been redirected.</p>
    <p>Click <a href="../Authenticated/Main.aspx">here</a> to go back to the main page.</p>
    </div>
    </form>
</body>
</html>
