<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Setup.ascx.cs" Inherits="CDSviewerDNN.Setup" %>

<script type="text/javascript" src="/DesktopModules/CDSviewerDNN/js/simplisity.js"></script>
<link rel="stylesheet" href="/DesktopModules/CDSviewerDNN/css/w3.css">

<link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:regular,bold,italic,thin,light,bolditalic,black,medium">
<link rel="stylesheet" href="https://fonts.googleapis.com/icon?family=Material+Icons">

<div class='w3-container w3-right'>
    <asp:linkbutton cssclass="cancelbutton w3-button w3-blue" id="cmdCancel" runat="server" ResourceKey="cmdReturn" causesvalidation="False" OnClick="cmdCancel_Click" />
</div>

<div id="simplisity_startpanel" class="w3-row simplisity_panel">
    <asp:PlaceHolder ID="adminpanel" runat="server"></asp:PlaceHolder>
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
        $(document).simplisityStartUp('/Desktopmodules/CDSviewerDNN/apihandler.ashx', '{"debug":"false"}');
    });
</script>


