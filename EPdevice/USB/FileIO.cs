using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace EPdevice
{
	///  <summary>
	///  This class provides declarations relating to file I/O used by WinUsb.
	///  </summary>

	sealed internal class FileIO
	{
		internal const Int32 INVALID_HANDLE_VALUE = -1;

		// dwDesiredAccess: The access to the object, which can be summarized as read, write, both, or neither (zero).
		internal const UInt32 GENERIC_READ = 0X80000000;
		internal const UInt32 GENERIC_WRITE = 0X40000000;

		// dwShareMode: The sharing mode of an object, which can be read, write, both, delete, all of these, or none.
		internal const Int32 FILE_SHARE_READ = 1;
		internal const Int32 FILE_SHARE_WRITE = 2;

		// dwCreationDisposition: An action to take on files that exist and do not exist.
		internal const Int32 OPEN_EXISTING = 3;

		// dwFlagsAndAttributes: The file attributes and flags. This parameter can include any combination of the available file attributes and flags.
		internal const Int32 FILE_ATTRIBUTE_NORMAL = 0X80;
		internal const Int32 FILE_FLAG_OVERLAPPED = 0X40000000;

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		internal static extern SafeFileHandle CreateFile(String lpFileName, UInt32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, Int32 hTemplateFile);
	}
}