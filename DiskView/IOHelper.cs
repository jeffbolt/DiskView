using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace DiskView
{
	public static class IOHelper
	{
		public static string GetBaseDirectory()
		{
			//if (Debugger.IsAttached)
			//	return Directory.GetCurrentDirectory();
			//else
				return Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
		}

		public static string GetTempDirectory()
		{
			return Path.Combine(Directory.GetParent(GetBaseDirectory()).Parent.FullName, "Temp");
		}

		public static string GetFileExtension(string fileName)
		{
			var objFileInfo = new FileInfo(fileName);
			return objFileInfo.Extension;
		}

		public static string GetMimeContentType(string mimeType)
		{
			// Return the file extension part of the MIME string (i.e. "image/pjpeg")
			var arrKeys = mimeType.Split('/');
			if (arrKeys.Length == 2)
				return arrKeys[1].Trim();
			else
				return "";
		}
	}
}