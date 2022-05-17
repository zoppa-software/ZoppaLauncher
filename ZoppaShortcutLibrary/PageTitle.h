#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections::Generic;

	/// <summary>�y�[�W�^�C�g�����B</summary>
	public ref class PageTitle sealed :
		IComparable<PageTitle^>
	{
	private:
		// �C���f�b�N�X
		int _index;

		// �^�C�g��
		String^ _title;

	public:
		/// <summary>�R���X�g���N�^�B</summary>
		/// <param name="index">�A�C�R���C���f�b�N�X�B</param>
		/// <param name="page">�y�[�W�B</param>
		PageTitle(int index, String^ title) : 
			_index(index), _title(title) 
		{}

	public:
		/// <summary>��r�����B</summary>
		/// <param name="other">��r�ΏہB</param>
		/// <returns>��r���ʁB</returns>
		virtual int CompareTo(PageTitle^ other) {
			return this->_index.CompareTo(other->_index);
		}

	public:
		/// <summary>�y�[�W�C���f�b�N�X���擾���܂��B</summary>
		property int Index {
			int get() { return this->_index; }
		}

		/// <summary>�y�[�W�^�C�g�����擾���܂��B</summary>
		property String^ Title {
			String^ get() { return this->_title; }
		}

	};

}