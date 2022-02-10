<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Skin" %>

<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="USER" Src="~/Admin/Skins/User.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGIN" Src="~/Admin/Skins/Login.ascx" %>
<%@ Register TagPrefix="dnn" TagName="jQuery" src="~/Admin/Skins/jQuery.ascx" %>


<div class='w3-display-container'>

	<div class="w3-row w3-display-bottomleft" style='z-index: 5;'>
		<div class="w3-left w3-padding">
			<dnn:LANGUAGE runat="server" id="LANGUAGE1"  showMenu="False" showLinks="True" />
		</div>
		<div id="login" class="w3-right w3-padding">
			<dnn:LOGIN ID="dnnLogin" CssClass="w3-button" runat="server" LegacyMode="false" />
		</div>
	</div>

	<div id="ContentPane" runat="server" valign="top" class="w3-row"></div>

</div>
