<%@ Page Language="C#" MasterPageFile="~masterurl/default.master" Inherits="Microsoft.SharePoint.WebPartPages.WebPartPage,Microsoft.SharePoint,Version=14.0.0.0,Culture=neutral,PublicKeyToken=71e9bce111e9429c" meta:progid="SharePoint.WebPartPage.Document" meta:webpartpageexpansion="full" %>

<%@ Register TagPrefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register TagPrefix="spellabsWebPartLibrary" Namespace="spellabsWebPartLibrary" Assembly="spellabsWebPartLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=87dfd736cac80cbd"%>


<asp:Content ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
	&nbsp;&nbsp;&nbsp; Найти документ
</asp:Content>
<asp:Content ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server">
	
</asp:Content>


<asp:Content ContentPlaceHolderID="PlaceHolderNewDocument" runat="server">

</asp:Content>

<asp:Content ContentPlaceHolderID="PlaceHolderMain" runat="server">
	<link rel="stylesheet" href="/_layouts/15/spellabs_shared/x5.quality/css/find-documents.css" />	
	<script type="text/javascript" src="/_LAYOUTS/15/spellabs_shared/x5.quality/js/find-documents.js"></script>
	<div id ="DispFormContainer" style="display:none"></div>
	<div id="PLUCardContainer" style="display:none"></div>
	<div id="documentsSearchFiltersDiv">
		<spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart 
				runat="server" 
				id="FiltersWebPart"
				Query="%query"	
				XsltUrl="..\..\x5.quality\xslt\site-find-document-filters.xsl"
				AjaxCode="" 
				DebugMode="False" 
				DoUpdateParameters="True" 
				AjaxBehaviour="#preload" 
				CustomParameters="" 
				__MarkupType="vsattributemarkup"
				WebPart="true" 
				__designer:IsClosed="false" __WebPartId="{E62E9F64-362C-45E2-B818-C35A5464BE8B}">
		</spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart >
		
		<spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart 
			runat="server" 
			id="PLUWebPart"
			Query="%query"	
			XsltUrl="..\..\x5.quality\xslt\site-find-document-plu.xsl"
			AjaxCode="PLUWebPart" 
			DebugMode="True" 
			DoUpdateParameters="True" 
			AjaxDefaultValue="" 
			AjaxBehaviour="#preload" 
			CustomParameters="" 
			__MarkupType="vsattributemarkup"
			__WebPartId="{38000cd0-f369-4c7a-9966-a6d0103b78b0}" 
			WebPart="true" 
			__designer:IsClosed="false">
		</spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart>

		<spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart 
			runat="server" 
			id="DocumentsWebPart"
			Query="%query"	
			XsltUrl="..\..\x5.quality\xslt\site-find-document-documentsList.xsl"
			AjaxCode="DocumentsWebPart" 
			includerequestdata="True"
			DebugMode="True"
			DoUpdateParameters="True" 
			AjaxDefaultValue="" 
			AjaxBehaviour="#preload" 
			CustomParameters="" 
			__MarkupType="vsattributemarkup"
			__WebPartId="{38000cd0-f369-4c7a-9966-a6d0103b78b0}" 
			WebPart="true" 
			__designer:IsClosed="false">
		</spellabsWebPartLibrary:SqlQueryXsltTransformerWebPart>
	</div>
	
	


</asp:Content>

