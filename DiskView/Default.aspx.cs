using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DiskView
{
	public partial class Default : System.Web.UI.Page
	{
		private DataSet ds;
		private DataView dv;
		private string rootFolder = IOHelper.GetBaseDirectory();
		private Dictionary<IntPtr, string> SystemIcons = new Dictionary<IntPtr, string>();

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!Page.IsPostBack)
			{
				if (Request.UrlReferrer != null && Request.UrlReferrer.ToString().Contains("Default.aspx"))
				{
					// New Search initited -> Reset the current page index
					ViewState["ViewState"] = null;
				}

				var dt = new DataTable();
				dt.Columns.Add("Name", typeof(string));
				dt.Columns.Add("Date Modified", typeof(DateTime));
				dt.Columns.Add("Type", typeof(string));
				dt.Columns.Add("Size", typeof(long));
				dt.Columns.Add("Folder", typeof(string));

				rootFolder = @"C:\Users\jbolt\OneDrive - Wiley\Documents\Graphics\Icons and Cursors";
				foreach (string dirname in Directory.GetDirectories(rootFolder))
				{
					Icon icon;
					var directoryInfo = new DirectoryInfo(dirname);
					string desktopIni = Path.Combine(directoryInfo.FullName, "desktop.ini");
					if (File.Exists(desktopIni))
					{
						string[] contents = File.ReadAllLines(desktopIni);
						if (contents.Contains("[.ShellClassInfo]"))
						{
							string line = contents.FirstOrDefault(x => x.StartsWith("IconResource="));
							if (!string.IsNullOrEmpty(line))
							{
								/* Ex:	IconResource=%SystemRoot%\system32\imageres.dll,-198
										IconResource=C:\Windows\explorer.exe,1
										IconResource=C:\Users\jbolt\Documents\Visual Studio 2017\folder.ico,0
								*/
								string iconResource = line.Split('=')[1];
								var resource = iconResource.Split(',');
								string iconPath = resource[0];
								if (iconPath.StartsWith("%"))
								{
									// Replace any system environment variables, i.e. %SystemRoot%\ = "C:\\WINDOWS"
									string sysVar = iconPath.Substring(0, iconPath.LastIndexOf("%") + 1);
									string envVar = sysVar.Replace("%", "");
									iconPath = iconPath.Replace(sysVar, Environment.GetEnvironmentVariable(envVar));
								}

								int iconNumber = int.Parse(resource[1]);

								if (File.Exists(iconPath))
								{
									var fi = new FileInfo(iconPath);
									switch (fi.Extension.ToUpper())
									{
										case ".DLL":
										case ".EXE":
										case ".ICL":
											// Extract icon from binary file
											// TODO: Why does this return null?
											icon = PInvoke.GetIconFromExe(iconPath, iconNumber.ToString(), 16);
											break;

										case ".ICO":
											// Load from .ico file
											icon = new Icon(iconPath, 16, 16);
											break;
									}
								}
							}
						}
					}

					foreach (FileInfo fileInfo in directoryInfo.GetFiles())
					{
						DataRow dr = dt.NewRow();
						dr["Name"] = fileInfo.Name;
						dr["Date Modified"] = fileInfo.LastWriteTime;  // Sortable as date
						dr["Type"] = PInvoke.GetFileType(fileInfo.FullName);
						dr["Size"] = fileInfo.Length;  // Sortable as numeric
						dr["Folder"] = fileInfo.Directory.FullName;
						dt.Rows.Add(dr);
					}
				}

				ds = new DataSet();
				ds.Tables.Add(dt);
				dv = new DataView(dt);
				dgFiles.DataSource = dv;
				dgFiles.DataBind();

				ViewState.Add("DataSource", ds);
			}
			else
			{
				ds = (DataSet)ViewState["DataSource"];
				dv = new DataView(ds.Tables[0]);
				dgFiles.DataSource = dv;

				// Apply sort expression
				if (dgFiles.AllowSorting && ViewState["SortExpression"] != null)
					((DataView)dgFiles.DataSource).Sort = (string)ViewState["SortExpression"];

				// Move to current page index
				if (dgFiles.AllowPaging && ViewState["CurrentPageIndex"] != null)
					dgFiles.CurrentPageIndex = (int)ViewState["CurrentPageIndex"];

				dgFiles.DataBind();
			}
		}
		
		protected void dgSearchResults_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
			{
				string filePath = Path.Combine(e.Item.Cells[4].Text, e.Item.Cells[0].Text);
				string dataUri = Properties.Settings.Default.DefaultFileDataUri;

				using (Icon icon = PInvoke.GetIcon(filePath, true))
				{
					if (SystemIcons.ContainsKey(icon.Handle))
					{
						dataUri = SystemIcons[icon.Handle];
					}
					else
					{
						using (Bitmap bmp = icon.ToBitmap())
						{
							int width = bmp.Width;
							int height = bmp.Height;
							using (var ms = new MemoryStream())
							{
								bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
								string base64 = Convert.ToBase64String(ms.GetBuffer());
								dataUri = $"data:image/png;base64,{base64}";
								SystemIcons.Add(icon.Handle, dataUri);
							}
						}
					}
				}
			
				e.Item.Cells[0].Text = $"<div><img src='{dataUri}' style='padding: 0 4px 0 2px' />{e.Item.Cells[0].Text}</div>";

				if (DateTime.TryParse(e.Item.Cells[1].Text, out DateTime dt))
					e.Item.Cells[1].Text = dt.ToString("g");
				if (long.TryParse(e.Item.Cells[3].Text, out long size))
					e.Item.Cells[3].Text = IOHelper.GetFileSizeSuffix(size);
			}
		}

		protected void dgFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void ResetColumnHeadings()
		{
			dgFiles.Columns[0].HeaderText = "Name";
			dgFiles.Columns[1].HeaderText = "Date Modified";
			dgFiles.Columns[2].HeaderText = "Type";
			dgFiles.Columns[3].HeaderText = "Size";
			dgFiles.Columns[4].HeaderText = "Folder";
		}

		protected void dgFiles_SortCommand(object sender, DataGridSortCommandEventArgs e)
		{
			ResetColumnHeadings();

			var lstColumns = new List<DataGridColumn>();
			foreach (DataGridColumn dgc in dgFiles.Columns)
				lstColumns.Add(dgc);

			var selected = lstColumns.FirstOrDefault(x => x.SortExpression == e.SortExpression);
			string sort = (string)(ViewState["SortExpression"] ?? "");

			if (sort.Contains(" DESC"))
			{
				sort = e.SortExpression.Replace(" DESC", "");
				selected.HeaderText += " " + FontAwesome.CaretUp;
			}
			else
			{
				sort = e.SortExpression + " DESC";
				selected.HeaderText += " " + FontAwesome.CaretDown;
			}

			dv.Sort = sort;
			ViewState["SortExpression"] = sort;
			dgFiles.DataSource = dv;
			dgFiles.DataBind();
		}

		protected void dgFiles_PageIndexChanged(object source, DataGridPageChangedEventArgs e)
		{
			dgFiles.CurrentPageIndex = e.NewPageIndex;
			dgFiles.DataBind();

			// Set ViewState variable to remember the last page index
			ViewState.Add("CurrentPageIndex", dgFiles.CurrentPageIndex);
		}
	}
}