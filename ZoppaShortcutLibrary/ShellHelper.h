#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;

	/// <summary>�V�F���R�s�[���b�p�[�B</summary>
	ref class ShellHelper
	{
	public:
		/// <summary>�t�@�C���R�s�[���s���B</summary>
		/// <param name="srcPath">�R�s�[���p�X�B</param>
		/// <param name="distPath">�R�s�[��p�X�B</param>
		/// <returns>�R�s�[���ʁB</returns>
		static bool Copy(String^ srcPath, String^ distPath);
	};

}