<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AudioUploader._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Audio Gallery</title>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="sm1" runat="server" />
        <div>
            Upload MP3 Audio:
            <asp:FileUpload ID="upload" runat="server" />
            <asp:Button ID="submitButton" runat="server" Text="Submit" OnClick="submitButton_Click" />
        </div>
        <div>
            <asp:UpdatePanel ID="up1" runat="server">
                <ContentTemplate>
                    <asp:ListView ID="AudioFileDisplayControl" runat="server">
                        <LayoutTemplate>
                            <tr ID="itemPlaceholder" runat="server"/>
                        </LayoutTemplate>
                        <ItemTemplate>
                            <table>
                            <tr>
                                <td>
                                    <audio src='<%# Eval("Url") %>' controls="" preload="none"></audio>
                                 </td>
                                <td>
                                    <asp:Literal ID="label" Text='<%# Eval("Title") %>' runat="server"/>
                                </td>
                            </tr>
                            </table>
                       </ItemTemplate>
                    </asp:ListView>
                    <asp:Timer ID="timer1" runat="server" Interval="30000" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div>
            <asp:Button ID="Button1" runat="server" Text="Refresh" />
        </div>
    </form>
</body>
</html>
