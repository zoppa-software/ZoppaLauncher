#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections;
	using namespace System::Collections::Generic;

	ref class PageTitle;
	ref class IconInformation;
	ref class LauncherPosition;

	/// <summary>アイコンコレクション。</summary>
	public ref class LauncherCollection sealed
	{
	public:
		/// <summary>位置とアイコン情報のペア。</summary>
		ref struct IconPair {
		public:
			/// <summary>位置情報。</summary>
			LauncherPosition^ position;

			/// <summary>アイコン情報。</summary>
			IconInformation^ informatition;
		};

	private:
		// ページタイトルリスト
		List<PageTitle^>^ _pages;

		// アイコン情報テーブル
		Dictionary<int, IconInformation^>^ _icons;

		// 位置情報リスト
		List<LauncherPosition^>^ _positions;

		// 縦横アイコン数
		int _wcount, _hcount;

		// ウィンドウカラー
		Color _foreColor, _backColor, _accentColor;

	public:
		/// <summary>コンストラクタ。</summary>
		LauncherCollection();

		/// <summary>設定ファイルを読み込む。</summary>
		/// <param name="doc">XMLドキュメント。</param>
		/// <returns>アイコンコレクション。</returns>
		static LauncherCollection^ Load(Xml::XmlDocument^ doc);

	public:
		/// <summary>XMLドキュメントに保存する。</summary>
		/// <returns>XMLドキュメント。</returns>
		Xml::XmlDocument^ Save();

		/// <summary>位置が使用されているか確認する。</summary>
		/// <param name="page">ページ。</param>
		/// <param name="row">行。</param>
		/// <param name="column">列。</param>
		/// <returns>使用されていたら真。</returns>
		bool UsedPosition(int page, int row, int column);

		/// <summary>アイコンの位置情報を更新する。</summary>
		/// <param name="page">ページ。</param>
		/// <param name="row">行。</param>
		/// <param name="column">列。</param>
		/// <param name="icon">アイコン情報。</param>
		void UpdateIcon(int page, int row, int column, IconInformation^ icon);

		/// <summary>指定ページのアイコン情報リストを取得する。</summary>
		/// <returns>アイコン情報リスト。</returns>
		List<LauncherCollection::IconPair^>^ Collect(int page);

		/// <summary>指定位置の情報を削除する。</summary>
		/// <param name="page">ページ。</param>
		/// <param name="row">行。</param>
		/// <param name="column">列。</param>
		void Remove(int page, int row, int column);

	private:
		/// <summary>指定位置以上の位置を検索する。</summary>
		/// <param name="key">指定位置。</param>
		/// <returns>指定位置以上の位置。</returns>
		int SearchPosition(LauncherPosition^ key);

		/// <summary>入力文字列を色へ変換する。</summary>
		/// <param name="inpstr">入力文字列。</param>
		/// <returns>色。</returns>
		static Color ConvertToColor(String^ inpstr);

		/// <summary>入力色を文字列へ変換する。</summary>
		/// <param name="inpclr">入力色。</param>
		/// <returns>文字列。</returns>
		static String^ ConvertToString(Color inpclr);

	public:
		/// <summary>横アイコン数を取得します。</summary>
		property int WCount {
			int get() { return this->_wcount; }
		}

		/// <summary>縦アイコン数を取得します。</summary>
		property int HCount {
			int get() { return this->_hcount; }
		}

		/// <summary>ページ数を取得します。</summary>
		property int PageCount {
			int get();
		}

		/// <summary>ウィンドウ前景色を取得します。</summary>
		property Color ForeColor {
			Color get() { return this->_foreColor; }
		}

		/// <summary>ウィンドウ背景色を取得します。</summary>
		property Color BackColor {
			Color get() { return this->_backColor; }
		}

		/// <summary>アクセントカラーを取得します。</summary>
		property Color AccentColor {
			Color get() { return this->_accentColor; }
		}
	};

}

