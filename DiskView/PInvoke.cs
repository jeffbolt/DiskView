using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace DiskView
{
	/*  Platform Invoke (P/Invoke)
        https://docs.microsoft.com/en-us/dotnet/standard/native-interop/pinvoke
        http://www.pinvoke.net/default.aspx/shell32/SHGetFileInfo.html
        https://stackoverflow.com/questions/1437382/get-file-type-in-net#1437451
    */
	public static class PInvoke
	{
		/// <summary>Maximal Length of unmanaged Windows-Path-strings</summary>
		private const int MAX_PATH = 260;
		/// <summary>Maximal Length of unmanaged Typename</summary>
		private const int MAX_TYPE = 80;

		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
			public string szTypeName;
		};

		//[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		//public struct SHFILEINFO
		//{
		//	public SHFILEINFO(bool b)
		//	{
		//		hIcon = IntPtr.Zero;
		//		iIcon = 0;
		//		dwAttributes = 0;
		//		szDisplayName = "";
		//		szTypeName = "";
		//	}
		//	public IntPtr hIcon;
		//	public int iIcon;
		//	public uint dwAttributes;
		//	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
		//	public string szDisplayName;
		//	[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_TYPE)]
		//	public string szTypeName;
		//};

		public static class FILE_ATTRIBUTE
		{
			public const uint FILE_ATTRIBUTE_NORMAL = 0x80;
		}

		//public static class SHGFI
		//{
		//	public const uint SHGFI_TYPENAME = 0x000000400;
		//	public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;
		//}

		[Flags]
		public enum SHGFI : uint
		{
			/// <summary>get icon</summary>
			Icon = 0x000000100,
			/// <summary>get display name</summary>
			DisplayName = 0x000000200,
			/// <summary>get type name</summary>
			TypeName = 0x000000400,
			/// <summary>get attributes</summary>
			Attributes = 0x000000800,
			/// <summary>get icon location</summary>
			IconLocation = 0x000001000,
			/// <summary>return exe type</summary>
			ExeType = 0x000002000,
			/// <summary>get system icon index</summary>
			SysIconIndex = 0x000004000,
			/// <summary>put a link overlay on icon</summary>
			LinkOverlay = 0x000008000,
			/// <summary>show icon in selected state</summary>
			Selected = 0x000010000,
			/// <summary>get only specified attributes</summary>
			Attr_Specified = 0x000020000,
			/// <summary>get large icon</summary>
			LargeIcon = 0x000000000,
			/// <summary>get small icon</summary>
			SmallIcon = 0x000000001,
			/// <summary>get open icon</summary>
			OpenIcon = 0x000000002,
			/// <summary>get shell size icon</summary>
			ShellIconSize = 0x000000004,
			/// <summary>pszPath is a pidl</summary>
			PIDL = 0x000000008,
			/// <summary>use passed dwFileAttribute</summary>
			UseFileAttributes = 0x000000010,
			/// <summary>apply the appropriate overlays</summary>
			AddOverlays = 0x000000020,
			/// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
			OverlayIndex = 0x000000040,
		}

		[DllImport("shell32.dll")]
		public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, out SHFILEINFO psfi, uint cbfileInfo, SHGFI uFlags);

		//public static string GetFileType(string filePath)
		//{
		//	var info = new SHFILEINFO();
		//	uint dwFileAttributes = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
		//	uint uFlags = (uint)(SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);

		//	SHGetFileInfo(filePath, dwFileAttributes, ref info, (uint)Marshal.SizeOf(info), uFlags);

		//	return info.szTypeName;
		//}

		public static string GetFileType(string filePath)
		{
			var info = new SHFILEINFO();
			uint dwFileAttributes = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
			uint uFlags = (uint)(SHGFI.TypeName | SHGFI.UseFileAttributes);

			SHGetFileInfo(filePath, dwFileAttributes, ref info, (uint)Marshal.SizeOf(info), uFlags);

			return info.szTypeName;
		}

		/// <summary>
		/// Returns the associated Icon for a file or application.
		/// If the file path is invalid or there is no icon, the default icon is returned.
		/// </summary>
		/// <param name="strPath">Full path to the file</param>
		/// <param name="bSmall">If true, the 16x16 icon is returned otherwise the 32x32</param>
		/// <returns></returns>
		public static Icon GetIcon(string strPath, bool bSmall)
		{
			SHFILEINFO info = new SHFILEINFO();
			int cbFileInfo = Marshal.SizeOf(info);
			SHGFI flags;
			if (bSmall)
				flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;
			else
				flags = SHGFI.Icon | SHGFI.LargeIcon | SHGFI.UseFileAttributes;

			SHGetFileInfo(strPath, 256, out info, (uint)cbFileInfo, flags);
			return Icon.FromHandle(info.hIcon);
		}
	}
}
