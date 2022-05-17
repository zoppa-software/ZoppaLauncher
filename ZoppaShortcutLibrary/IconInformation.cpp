#include "pch.h"
#include "IconInformation.h"

namespace ZoppaShortcutLibrary {

	using namespace System::IO;
	using namespace System::Windows;
	using namespace System::Windows::Media::Imaging;

	IconInformation::IconInformation(String^ name, String^ path, ImageSource^ image)
		: _name(name), _path(path), _image(image)
	{}

	IconInformation::~IconInformation() {
		if (this->_image) {
			delete this->_image;
			this->_image = nullptr;
		}
	}

	IconInformation^ IconInformation::Create(String^ path, String^ linkStockFolder) {
		// 作成したショートカットを保持するフォルダを作成する
		DirectoryInfo^ dinfo = gcnew DirectoryInfo(linkStockFolder);
		if (!dinfo->Exists) {
			Directory::CreateDirectory(dinfo->FullName);
		}

		// ショートカットを作成する
		FileInfo^ finfo = gcnew FileInfo(path);
		String^ sname = finfo->Name->Substring(0, finfo->Name->Length - finfo->Extension->Length);
		String^ newLinkPath = L"";
		if (finfo->Extension == L".lnk") {
			newLinkPath = dinfo->FullName + L"\\" + finfo->Name;
			if (finfo->FullName != newLinkPath) {
				File::Copy(finfo->FullName, newLinkPath, true);
			}
		}
		else {
			newLinkPath = dinfo->FullName + L"\\" + sname + L".lnk";
			CreateShortcut(newLinkPath, finfo->FullName);
		}

		// ショートカット画像を取得する
		ImageSource^ image = GetIconImage(newLinkPath, SHIL_EXTRALARGE);

		return gcnew IconInformation(sname, newLinkPath, image);
	}

	IconInformation^ IconInformation::Load(String^ name, String^ path) {
		ImageSource^ image = GetIconImage(path, SHIL_EXTRALARGE);
		return gcnew IconInformation(name, path, image);
	}

	void IconInformation::CreateShortcut(String^ linkPath, String^ srcPath) {
		IShellLink* sellLink = NULL;
		IPersistFile* persistFile = NULL;

		try {
			// IShellLinkの取得
			if (FAILED(CoCreateInstance(CLSID_ShellLink, NULL, CLSCTX_INPROC_SERVER, IID_IShellLink, (void**)&sellLink))) {
				throw gcnew InvalidOperationException(L"CoCreateInstance error");
			}

			// IPersistFileの取得
			if (FAILED(sellLink->QueryInterface(IID_IPersistFile, (void**)&persistFile))) {
				throw gcnew InvalidOperationException(L"QueryInterface error");
			}

			// リンク先パスの設定
			pin_ptr<const wchar_t> wstrPath = PtrToStringChars(srcPath);
			if (FAILED(sellLink->SetPath(wstrPath))) {
				throw gcnew InvalidOperationException(L"sellLink->SetPath error");
			}

			// ショートカットの保存
			pin_ptr<const wchar_t> wlinkPath = PtrToStringChars(linkPath);
			if (FAILED(persistFile->Save(wlinkPath, true))) {
				throw gcnew InvalidOperationException(L"persistFile->Save error");
			}
		}
		catch (Exception^) {
			throw;
		}
		finally {
			if (persistFile) { persistFile->Release(); }
			if (sellLink) { sellLink->Release(); }
		}
	}

	ImageSource^ IconInformation::GetIconImage(String^ path, int size) {
		ImageSource^ res = nullptr;

		System::Drawing::Icon^ img = nullptr;
		SHFILEINFO shinfo = {0};
		IImageList* iml = NULL;
		HICON hIcon = NULL;

		try {
			// アイコン情報を取得
			pin_ptr<const wchar_t> wpath = PtrToStringChars(path);
			if (FAILED(SHGetFileInfo(wpath, 0, &shinfo, sizeof(shinfo), SHGFI_ICON | SHGFI_LARGEICON))) {
				throw gcnew InvalidOperationException(L"SHGetFileInfo error");
			}

			// アイコン画像リスト領域を取得
			if (FAILED(SHGetImageList(size, IID_IImageList, (void**)&iml))) {
				throw gcnew InvalidOperationException(L"SHGetImageList error");
			}

			// アイコンリストを取得
			if (FAILED(iml->GetIcon(shinfo.iIcon, ILD_TRANSPARENT, &hIcon))) {
				if (shinfo.iIcon) { DestroyIcon(shinfo.hIcon); }
				throw gcnew InvalidOperationException(L"GetIcon error");
			}

			// アイコン画像を取得
			img = System::Drawing::Icon::FromHandle(IntPtr(hIcon));
			res = System::Windows::Interop::Imaging::CreateBitmapSourceFromHIcon(
				img->Handle, Int32Rect::Empty, BitmapSizeOptions::FromEmptyOptions()
			);
			res->Freeze();

			return res;

		}
		catch (Exception^) {
			throw;
		}
		finally {
			if (img) { delete img; }
			if (iml) { iml->Release(); }
			if (hIcon) { DestroyIcon(hIcon); }
		}
	}
}

