#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Collections;
	using namespace System::Collections::Generic;

	ref class PageTitle;
	ref class IconInformation;
	ref class LauncherPosition;

	/// <summary>�A�C�R���R���N�V�����B</summary>
	public ref class LauncherCollection sealed
	{
	public:
		/// <summary>�ʒu�ƃA�C�R�����̃y�A�B</summary>
		ref struct IconPair {
		public:
			/// <summary>�ʒu���B</summary>
			LauncherPosition^ position;

			/// <summary>�A�C�R�����B</summary>
			IconInformation^ informatition;
		};

	private:
		// �y�[�W�^�C�g�����X�g
		List<PageTitle^>^ _pages;

		// �A�C�R�����e�[�u��
		Dictionary<int, IconInformation^>^ _icons;

		// �ʒu��񃊃X�g
		List<LauncherPosition^>^ _positions;

		// �c���A�C�R����
		int _wcount, _hcount;

		// �E�B���h�E�J���[
		Color _foreColor, _backColor, _accentColor;

	public:
		/// <summary>�R���X�g���N�^�B</summary>
		LauncherCollection();

		/// <summary>�ݒ�t�@�C����ǂݍ��ށB</summary>
		/// <param name="doc">XML�h�L�������g�B</param>
		/// <returns>�A�C�R���R���N�V�����B</returns>
		static LauncherCollection^ Load(Xml::XmlDocument^ doc);

	public:
		/// <summary>XML�h�L�������g�ɕۑ�����B</summary>
		/// <returns>XML�h�L�������g�B</returns>
		Xml::XmlDocument^ Save();

		/// <summary>�ʒu���g�p����Ă��邩�m�F����B</summary>
		/// <param name="page">�y�[�W�B</param>
		/// <param name="row">�s�B</param>
		/// <param name="column">��B</param>
		/// <returns>�g�p����Ă�����^�B</returns>
		bool UsedPosition(int page, int row, int column);

		/// <summary>�A�C�R���̈ʒu�����X�V����B</summary>
		/// <param name="page">�y�[�W�B</param>
		/// <param name="row">�s�B</param>
		/// <param name="column">��B</param>
		/// <param name="icon">�A�C�R�����B</param>
		void UpdateIcon(int page, int row, int column, IconInformation^ icon);

		/// <summary>�w��y�[�W�̃A�C�R����񃊃X�g���擾����B</summary>
		/// <returns>�A�C�R����񃊃X�g�B</returns>
		List<LauncherCollection::IconPair^>^ Collect(int page);

		/// <summary>�w��ʒu�̏����폜����B</summary>
		/// <param name="page">�y�[�W�B</param>
		/// <param name="row">�s�B</param>
		/// <param name="column">��B</param>
		void Remove(int page, int row, int column);

	private:
		/// <summary>�w��ʒu�ȏ�̈ʒu����������B</summary>
		/// <param name="key">�w��ʒu�B</param>
		/// <returns>�w��ʒu�ȏ�̈ʒu�B</returns>
		int SearchPosition(LauncherPosition^ key);

		/// <summary>���͕������F�֕ϊ�����B</summary>
		/// <param name="inpstr">���͕�����B</param>
		/// <returns>�F�B</returns>
		static Color ConvertToColor(String^ inpstr);

		/// <summary>���͐F�𕶎���֕ϊ�����B</summary>
		/// <param name="inpclr">���͐F�B</param>
		/// <returns>������B</returns>
		static String^ ConvertToString(Color inpclr);

	public:
		/// <summary>���A�C�R�������擾���܂��B</summary>
		property int WCount {
			int get() { return this->_wcount; }
		}

		/// <summary>�c�A�C�R�������擾���܂��B</summary>
		property int HCount {
			int get() { return this->_hcount; }
		}

		/// <summary>�y�[�W�����擾���܂��B</summary>
		property int PageCount {
			int get();
		}

		/// <summary>�E�B���h�E�O�i�F���擾���܂��B</summary>
		property Color ForeColor {
			Color get() { return this->_foreColor; }
		}

		/// <summary>�E�B���h�E�w�i�F���擾���܂��B</summary>
		property Color BackColor {
			Color get() { return this->_backColor; }
		}

		/// <summary>�A�N�Z���g�J���[���擾���܂��B</summary>
		property Color AccentColor {
			Color get() { return this->_accentColor; }
		}
	};

}

