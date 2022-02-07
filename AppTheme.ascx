<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AppTheme.ascx.cs" Inherits="ToastedMod.AppTheme" %>

<script type="text/javascript" src="/DesktopModules/Toasted/ToastedMod/js/simplisity.js"></script>
<link rel="stylesheet" href="/DesktopModules/Toasted/ToastedMod/css/w3.css">

<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium">
<link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">

<div id="simplisity_startpanel" class="w3-row simplisity_panel">
    <asp:PlaceHolder ID="adminpanel" runat="server"></asp:PlaceHolder>
</div>

<div class='w3-container w3-right'>
    <asp:linkbutton cssclass="cancelbutton w3-button w3-blue" id="cmdCancel" runat="server" ResourceKey="cmdReturn" causesvalidation="False" OnClick="cmdCancel_Click" />
</div>

<style>
    #editBarContainer {
        display: none !important
    }

    .personalBarContainer {
        display: none !important
    }
    #Body {
        margin-left: 0px !important
    }
    .material-icons {
        vertical-align: middle;
    }
</style>

<script>
    $(document).ready(function () {       
        $(document).simplisityStartUp('/Desktopmodules/toasted/ToastedMod/apihandler.ashx', '{"debug":"false"}');
    });
</script>


