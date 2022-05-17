#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections::Generic;

	/// <summary>ページタイトル情報。</summary>
	public ref class PageTitle sealed :
		IComparable<PageTitle^>
	{
	private:
		// インデックス
		int _index;

		// タイトル
		String^ _title;

	public:
		/// <summary>コンストラクタ。</summary>
		/// <param name="index">アイコンインデックス。</param>
		/// <param name="page">ページ。</param>
		PageTitle(int index, String^ title) : 
			_index(index), _title(title) 
		{}

	public:
		/// <summary>比較処理。</summary>
		/// <param name="other">比較対象。</param>
		/// <returns>比較結果。</returns>
		virtual int CompareTo(PageTitle^ other) {
			return this->_index.CompareTo(other->_index);
		}

	public:
		/// <summary>ページインデックスを取得します。</summary>
		property int Index {
			int get() { return this->_index; }
		}

		/// <summary>ページタイトルを取得します。</summary>
		property String^ Title {
			String^ get() { return this->_title; }
		}

	};

}