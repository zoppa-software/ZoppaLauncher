#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections::Generic;

	/// <summary>アイコン位置情報。</summary>
	public ref class IconPosition sealed :
		IComparable<IconPosition^>
	{
	private:
		// ページ
		int _page;

		// 行
		int _row;

		// 列
		int _column;

		// アイコンインデックス
		int _index;

	public:
		/// <summary>コンストラクタ。</summary>
		/// <param name="page">ページ。</param>
		/// <param name="row">行。</param>
		/// <param name="column">列。</param>
		/// <param name="index">アイコンインデックス。</param>
		IconPosition(int page, int row, int column, int index);

	public:
		/// <summary>比較処理。</summary>
		/// <param name="other">比較対象。</param>
		/// <returns>比較結果。</returns>
		virtual int CompareTo(IconPosition^ other);

		/// <summary>ページインデックスを -1 する。</summary>
		void DecrementPage() {
			this->_page--;
		}

	private:
		/// <summary>ページ、行、列を取得します。</summary>
		/// <param name="index">位置の比較順。</param>
		/// <returns>位置情報。</returns>
		int GetOrder(int index);

	public:
		/// <summary>ページを取得します。</summary>
		property int Page {
			int get() { return this->_page; }
		}

		/// <summary>行を取得します。</summary>
		property int Row {
			int get() { return this->_row; }
		}

		/// <summary>列を取得します。</summary>
		property int Column {
			int get() { return this->_column; }
		}

		/// <summary>インデックスを取得します。</summary>
		property int Index {
			int get() { return this->_index; }
		}
	};

}

