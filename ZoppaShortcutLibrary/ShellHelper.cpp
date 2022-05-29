#include "pch.h"
#include <windows.h>
#include "ShellHelper.h"

namespace ZoppaShortcutLibrary {

	bool ShellHelper::Copy(String^ srcPath, String^ distPath)
	{
		if (srcPath != distPath) {
			pin_ptr<const wchar_t> wsrcPath = PtrToStringChars(srcPath);
			pin_ptr<const wchar_t> wdistPath = PtrToStringChars(distPath);

			SHFILEOPSTRUCT op = {};
			op.hwnd = NULL;
			op.wFunc = FO_COPY;
			op.pFrom = wsrcPath;
			op.pTo = wdistPath;
			op.fFlags = FOF_NOCONFIRMATION | FOF_NOCONFIRMMKDIR | FOF_NOERRORUI;

			return !SHFileOperation(&op);
		}
		else {
			return false;
		}
	}
}