#pragma once

namespace ZoppaShortcutLibrary {

	using namespace System;
	using namespace System::Windows::Media;

	/// <summary>�A�C�R�����B</summary>
	public ref class IconInformation sealed :
		IDisposable
	{
	private:
		/// <summary>�t�@�C�����B</summary>
		ref class FileResult {
		public:
			/// <summary>�\�����B</summary>
			String^ name;

			/// <summary>�A�C�R���摜�B</summary>
			ImageSource^ image;
		};

	private:
		// ���O
		String^ _name;

		// �V���[�g�J�b�g�p�X
		String^ _path;

		// �摜���
		ImageSource^ _image;

	private:
		/// <summary>�R���X�g���N�^�B</summary>
		/// <param name="name">�A�C�R�����B</param>
		/// <param name="path">�V���[�g�J�b�g�p�X�B</param>
		/// <param name="img">�A�C�R���摜�B</param>
		IconInformation(String^ name, String^ path, ImageSource^ image);

		/// <summary>�f�X�g���N�^�B</summary>
		virtual ~IconInformation();

	public:
		/// <summary>�A�C�R�������쐬����B</summary>
		/// <param name="path">���s�t�@�C���p�X�B</param>
		/// <param name="linkStockFolder">�V���[�g�J�b�g�ۑ��t�H���_�p�X�B</param>
		/// <returns>�A�C�R�����B</returns>
		static IconInformation^ Create(String^ path, String^ linkStockFolder);

		/// <summary>�A�C�R������ǂݍ��ށB</summary>
		/// <param name="path">�A�C�R���p�X�B</param>
		/// <returns>�A�C�R�����B</returns>
		static IconInformation^ Load(String^  name, String^ path);

	private:
		/// <summary>�V���[�g�J�b�g�̃p�X���쐬����B</summary>
		/// <param name="linkPath">�쐬����V���[�g�J�b�g�p�X�B</param>
		/// <param name="srcPath">�쐬���̎��s�t�@�C���p�X�B</param>
		static void CreateShortcut(String^ linkPath, String^ srcPath);

		/// <summary>�t�@�C�������擾����B</summary>
		/// <param name="path">�V���[�g�J�b�g�p�X�B</param>
		/// <param name="size">�摜�T�C�Y�B</param>
		/// <returns>�t�@�C�����B</returns>
		static FileResult^ GetIconImage(String^ path, int size);

	public:
		/// <summary>�A�C�R�������擾����B</summary>
		property String^ Name {
			String^ get() { return this->_name; }
		}

		/// <summary>�V���[�g�J�b�g�p�X���擾����B</summary>
		property String^ ShortcutPath {
			String^ get() { return this->_path; }
		}

		/// <summary>�A�C�R���摜���擾����B</summary>
		property ImageSource^ ShortcutImage {
			ImageSource^ get() { return this->_image; }
		}
	};

}

