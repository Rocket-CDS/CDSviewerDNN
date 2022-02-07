<%@ Control Language="C#" AutoEventWireup="false" Inherits="DotNetNuke.UI.Skins.Skin" %>

<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="USER" Src="~/Admin/Skins/User.ascx" %>
<%@ Register TagPrefix="dnn" TagName="LOGIN" Src="~/Admin/Skins/Login.ascx" %>
<%@ Register TagPrefix="dnn" TagName="jQuery" src="~/Admin/Skins/jQuery.ascx" %>


<div class="w3-row w3-grey">
		<div class="w3-left">
			<dnn:LANGUAGE runat="server" id="LANGUAGE1" showMenu="False" showLinks="True" />
		</div>
		<div id="login" class="w3-right">
			<dnn:LOGIN ID="dnnLogin" CssClass="w3-button" runat="server" LegacyMode="false" />
		</div>
</div>

<div class="w3-container w3-padding-64">
   
    <div id="ContentPane" class="contentPane" runat="server"></div>    

</div>

