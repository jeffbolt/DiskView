﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DiskView.Default" %>

<asp:Content ID="cphHead" ContentPlaceHolderID="cphHead" runat="server">
</asp:Content>
<asp:Content ID="cphBody" ContentPlaceHolderID="cphBody" runat="server">
	<p>Files</p>
	<div style="font-size: 10pt">
		<asp:UpdatePanel ID="upFiles" runat="server">
			<ContentTemplate>
				<asp:DataGrid ID="dgFiles" runat="server" CssClass="datagrid" AllowSorting="True" AutoGenerateColumns="False" AllowPaging="True"
					OnSelectedIndexChanged="dgFiles_SelectedIndexChanged" OnSortCommand="dgFiles_SortCommand" OnPageIndexChanged="dgFiles_PageIndexChanged" 
					ForeColor="#333333" GridLines="None">
					<HeaderStyle BackColor="#5D7B9D" ForeColor="White" CssClass="semi-bold" />
					<ItemStyle BackColor="#F7F6F3" ForeColor="#333333" />
					<AlternatingItemStyle BackColor="White" ForeColor="#284775" />
					<SelectedItemStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
					<FooterStyle BackColor="#5D7B9D" ForeColor="White" />
					<PagerStyle PageButtonCount="50" Mode="NumericPages" BackColor="Gainsboro" ForeColor="#5D7B9D" Position="Bottom" CssClass="semi-bold" />
					<Columns>
						<asp:BoundColumn DataField="File Name" HeaderText="File Name" SortExpression="File Name">
							<ItemStyle Width="200px" Wrap="false" />
						</asp:BoundColumn>
						<asp:BoundColumn DataField="Folder" HeaderText="Folder" SortExpression="Folder">
							<ItemStyle Wrap="false" />
						</asp:BoundColumn>
					</Columns>
				</asp:DataGrid>
			</ContentTemplate>
		</asp:UpdatePanel>
	</div>
</asp:Content>
