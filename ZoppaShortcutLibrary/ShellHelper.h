#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;

	/// <summary>シェルコピーラッパー。</summary>
	ref class ShellHelper
	{
	public:
		/// <summary>ファイルコピーを行う。</summary>
		/// <param name="srcPath">コピー元パス。</param>
		/// <param name="distPath">コピー先パス。</param>
		/// <returns>コピー結果。</returns>
		static bool Copy(String^ srcPath, String^ distPath);
	};

}