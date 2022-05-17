#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections::Generic;

	/// <summary>�A�C�R���ʒu���B</summary>
	public ref class IconPosition sealed :
		IComparable<IconPosition^>
	{
	private:
		// �y�[�W
		int _page;

		// �s
		int _row;

		// ��
		int _column;

		// �A�C�R���C���f�b�N�X
		int _index;

	public:
		/// <summary>�R���X�g���N�^�B</summary>
		/// <param name="page">�y�[�W�B</param>
		/// <param name="row">�s�B</param>
		/// <param name="column">��B</param>
		/// <param name="index">�A�C�R���C���f�b�N�X�B</param>
		IconPosition(int page, int row, int column, int index);

	public:
		/// <summary>��r�����B</summary>
		/// <param name="other">��r�ΏہB</param>
		/// <returns>��r���ʁB</returns>
		virtual int CompareTo(IconPosition^ other);

		/// <summary>�y�[�W�C���f�b�N�X�� -1 ����B</summary>
		void DecrementPage() {
			this->_page--;
		}

	private:
		/// <summary>�y�[�W�A�s�A����擾���܂��B</summary>
		/// <param name="index">�ʒu�̔�r���B</param>
		/// <returns>�ʒu���B</returns>
		int GetOrder(int index);

	public:
		/// <summary>�y�[�W���擾���܂��B</summary>
		property int Page {
			int get() { return this->_page; }
		}

		/// <summary>�s���擾���܂��B</summary>
		property int Row {
			int get() { return this->_row; }
		}

		/// <summary>����擾���܂��B</summary>
		property int Column {
			int get() { return this->_column; }
		}

		/// <summary>�C���f�b�N�X���擾���܂��B</summary>
		property int Index {
			int get() { return this->_index; }
		}
	};

}

