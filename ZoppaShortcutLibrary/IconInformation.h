#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Windows::Media;

	/// <summary>アイコン情報。</summary>
	public ref class IconInformation sealed :
		IDisposable
	{
	private:
		/// <summary>ファイル情報。</summary>
		ref class FileResult {
		public:
			/// <summary>表示名。</summary>
			String^ name;

			/// <summary>アイコン画像。</summary>
			ImageSource^ image;
		};

	private:
		// 名前
		String^ _name;

		// ショートカットパス
		String^ _path;

		// 画像情報
		ImageSource^ _image;

	private:
		/// <summary>コンストラクタ。</summary>
		/// <param name="name">アイコン名。</param>
		/// <param name="path">ショートカットパス。</param>
		/// <param name="img">アイコン画像。</param>
		IconInformation(String^ name, String^ path, ImageSource^ image);

		/// <summary>デストラクタ。</summary>
		virtual ~IconInformation();

	public:
		/// <summary>アイコン情報を作成する。</summary>
		/// <param name="path">実行ファイルパス。</param>
		/// <param name="linkStockFolder">ショートカット保存フォルダパス。</param>
		/// <returns>アイコン情報。</returns>
		static IconInformation^ Create(String^ path, String^ linkStockFolder);

		/// <summary>アイコン情報を読み込む。</summary>
		/// <param name="path">アイコンパス。</param>
		/// <returns>アイコン情報。</returns>
		static IconInformation^ Load(String^  name, String^ path);

	private:
		/// <summary>ショートカットのパスを作成する。</summary>
		/// <param name="linkPath">作成するショートカットパス。</param>
		/// <param name="srcPath">作成元の実行ファイルパス。</param>
		static void CreateShortcut(String^ linkPath, String^ srcPath);

		/// <summary>ファイル情報を取得する。</summary>
		/// <param name="path">ショートカットパス。</param>
		/// <param name="size">画像サイズ。</param>
		/// <returns>ファイル情報。</returns>
		static FileResult^ GetIconImage(String^ path, int size);

	public:
		/// <summary>アイコン名を取得する。</summary>
		property String^ Name {
			String^ get() { return this->_name; }
		}

		/// <summary>ショートカットパスを取得する。</summary>
		property String^ ShortcutPath {
			String^ get() { return this->_path; }
		}

		/// <summary>アイコン画像を取得する。</summary>
		property ImageSource^ ShortcutImage {
			ImageSource^ get() { return this->_image; }
		}
	};

}

