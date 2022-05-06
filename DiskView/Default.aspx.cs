using System;
using System.Collections.Generic;
using System.Data;
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
		private readonly string rootFolder = IOHelper.GetBaseDirectory();

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
				dt.Columns.Add("File Name", typeof(string));
				dt.Columns.Add("Folder", typeof(string));

				foreach (string dirname in Directory.GetDirectories(rootFolder))
				{
					var directoryInfo = new DirectoryInfo(dirname);
					foreach (FileInfo fileInfo in directoryInfo.GetFiles())
					{
						DataRow dr = dt.NewRow();
						dr["File Name"] = fileInfo.Name;
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
		
		protected void dgFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void ResetColumnHeadings()
		{
			dgFiles.Columns[0].HeaderText = "File Name";
			dgFiles.Columns[1].HeaderText = "Folder";
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
			try
			{
				dgFiles.CurrentPageIndex = e.NewPageIndex;
				dgFiles.DataBind();

				// Set ViewState variable to remember the last page index
				ViewState.Add("CurrentPageIndex", dgFiles.CurrentPageIndex);
			}
			catch (NullReferenceException)
			{
			}
			catch (Exception)
			{
			}
		}
	}
}