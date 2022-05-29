#include "pch.h"
#include "PageTitle.h"
#include "IconInformation.h"
#include "LauncherPosition.h"
#include "LauncherCollection.h"

namespace ZoppaShortcutLibrary {

	LauncherCollection::LauncherCollection() :
		_pages(gcnew List<PageTitle^>()),
		_icons(gcnew Dictionary<int, IconInformation^>()), 
		_positions(gcnew List<LauncherPosition^>()),
		_wcount(5),
		_hcount(5),
		_foreColor(Color::FromArgb(255, 250, 250, 250)),
		_backColor(Color::FromArgb(230, 26, 119, 189)),
		_accentColor(Color::FromArgb(255, 250, 250, 250))
	{}

	LauncherCollection^ LauncherCollection::Load(Xml::XmlDocument^ doc) {
		LauncherCollection^ res = gcnew LauncherCollection();

		// ページリストを取得する
		res->_pages = gcnew List<PageTitle^>();
		for each (Xml::XmlNode ^ node in doc->SelectNodes(L"setting/pages/page")) {
			int index = Convert::ToInt32(node->Attributes[L"index"]->Value);
			String^ title = node->Attributes[L"title"]->Value;
			res->_pages->Add(gcnew PageTitle(index, title));
		}
		res->_pages->Sort();

		// アイコン数を取得する
		Xml::XmlNode^ root = doc->SelectSingleNode(L"setting");
		if (root != nullptr) {
			res->_wcount = Convert::ToInt32(root->Attributes[L"wcount"]->Value);
			res->_hcount = Convert::ToInt32(root->Attributes[L"hcount"]->Value);
			res->_foreColor = ConvertToColor(root->Attributes[L"foreColor"]->Value);
			res->_backColor = ConvertToColor(root->Attributes[L"backColor"]->Value);
			res->_accentColor = ConvertToColor(root->Attributes[L"accentColor"]->Value);
		}

		// アイコン情報リストを取得する
		res->_icons = gcnew Dictionary<int, IconInformation^>();
		for each (Xml::XmlNode^ node in doc->SelectNodes(L"setting/icons/icon")) {
			int index = Convert::ToInt32(node->Attributes[L"index"]->Value);
			String^ name = node->Attributes[L"name"]->Value;
			String^ path = node->Attributes[L"path"]->Value;
			if (!res->_icons->ContainsKey(index)) {
				res->_icons->Add(index, IconInformation::Load(name, path));
			}
		}

		// 位置情報リストを取得する
		res->_positions = gcnew List<LauncherPosition^>();
		for each (Xml::XmlNode ^ node in doc->SelectNodes(L"setting/positions/position")) {
			int page = Convert::ToInt32(node->Attributes[L"page"]->Value);
			int row = Convert::ToInt32(node->Attributes[L"row"]->Value);
			int column = Convert::ToInt32(node->Attributes[L"column"]->Value);
			int index = Convert::ToInt32(node->Attributes[L"index"]->Value);
			res->_positions->Add(gcnew LauncherPosition(page, row, column, index));
		}
		res->_positions->Sort();

		return res;
	}

	Xml::XmlDocument^ LauncherCollection::Save() {
		Xml::XmlDocument^ doc = gcnew Xml::XmlDocument();

		// ヘッダ要素を追加
		Xml::XmlDeclaration^ decla = doc->CreateXmlDeclaration(L"1.0", "UTF-8", nullptr);
		doc->AppendChild(decla);
		
		// ルート要素を追加
		Xml::XmlElement^ settingEle = doc->CreateElement(L"setting");
		settingEle->SetAttribute(L"wcount", String::Format(L"{0}", this->_wcount));
		settingEle->SetAttribute(L"hcount", String::Format(L"{0}", this->_hcount));
		settingEle->SetAttribute(L"foreColor", ConvertToString(this->_foreColor));
		settingEle->SetAttribute(L"backColor", ConvertToString(this->_backColor));
		settingEle->SetAttribute(L"accentColor", ConvertToString(this->_accentColor));
		doc->AppendChild(settingEle);

		// ページ情報を保存する
		Xml::XmlElement^ pagesEle = doc->CreateElement(L"pages");
		for each (PageTitle ^ pg in this->_pages) {
			Xml::XmlElement^ ele = doc->CreateElement(L"page");
			ele->SetAttribute(L"index", String::Format(L"{0}", pg->Index));
			ele->SetAttribute(L"title", pg->Title);
			pagesEle->AppendChild(ele);
		}
		settingEle->AppendChild(pagesEle);

		// アイコン情報を保存する
		Xml::XmlElement^ iconsEle = doc->CreateElement(L"icons");
		for each (int index in this->_icons->Keys) {
			IconInformation^ icon = this->_icons[index];

			Xml::XmlElement^ ele = doc->CreateElement(L"icon");
			ele->SetAttribute(L"index", String::Format(L"{0}", index));
			ele->SetAttribute(L"name", icon->Name);
			ele->SetAttribute(L"path", icon->ShortcutPath);
			iconsEle->AppendChild(ele);
		}
		settingEle->AppendChild(iconsEle);

		// 位置情報を保存する
		Xml::XmlElement^ possEle = doc->CreateElement(L"positions");
		for each (LauncherPosition^ pos in this->_positions) {
			Xml::XmlElement^ ele = doc->CreateElement(L"position");
			ele->SetAttribute(L"page", String::Format(L"{0}", pos->Page));
			ele->SetAttribute(L"row", String::Format(L"{0}", pos->Row));
			ele->SetAttribute(L"column", String::Format(L"{0}", pos->Column));
			ele->SetAttribute(L"index", String::Format(L"{0}", pos->Index));
			possEle->AppendChild(ele);
		}
		settingEle->AppendChild(possEle);

		return doc;
	}

	bool LauncherCollection::UsedPosition(int page, int row, int column) {
		LauncherPosition^ key = gcnew LauncherPosition(page, row, column, 0);
		int idx = this->SearchPosition(key);
		if (idx >= 0 && idx < this->_positions->Count) {
			return (this->_positions[idx]->CompareTo(key) == 0);
		}
		return false;
	}

	void LauncherCollection::UpdateIcon(int page, int row, int column, IconInformation^ icon) {
		// アイコンのインデックスリストを作成
		List<int>^ keys = gcnew List<int>(this->_icons->Keys);
		keys->Sort();

		// 追加するインデックスを取得
		int insert = this->_positions->Count;
		for (int i = 0; i < this->_positions->Count; ++i) {
			if (keys[i] != i) {
				insert = i;
				break;
			}
		}

		// インスタンスに位置とアイコン情報を追加
		this->_positions->Add(gcnew LauncherPosition(page, row, column, insert));
		this->_positions->Sort();
		this->_icons->Add(insert, icon);
	}

	List<LauncherCollection::IconPair^>^ LauncherCollection::Collect(int page) {
		List<LauncherCollection::IconPair^>^ res = gcnew List<LauncherCollection::IconPair^>();

		LauncherPosition^ key = gcnew LauncherPosition(page, 0, 0, 0);
		int idx = this->SearchPosition(key);
		for (int i = idx; i < this->_positions->Count; ++i) {
			LauncherPosition^ pos = this->_positions[i];

			if (pos->Page == page) {
				LauncherCollection::IconPair^ pair = gcnew LauncherCollection::IconPair();
				pair->position = pos;
				pair->informatition = this->_icons[pos->Index];
				res->Add(pair);
			}
		}

		return res;
	}

	void LauncherCollection::Remove(int page, int row, int column) {
		LauncherPosition^ key = gcnew LauncherPosition(page, row, column, 0);

		int idx = this->SearchPosition(key);
		if (idx >= 0 && 
			idx < this->_positions->Count && 
			this->_positions[idx]->CompareTo(key) == 0) {
			// 登録済みの位置を取得
			LauncherPosition^ pos = this->_positions[idx];

			// インスタンスから情報を削除する
			this->_positions->RemoveAt(idx);
			this->_icons->Remove(pos->Index);

			// ページを調整する
			LauncherPosition^ topkey = gcnew LauncherPosition(page, 0, 0, 0);
			int topidx = this->SearchPosition(topkey);
			if (topidx >= 0 &&
				topidx < this->_positions->Count &&
				this->_positions[topidx]->Page > page) {
				// 後方位置のページを -1
				for (int i = topidx; i < this->_positions->Count; ++i) {
					this->_positions[i]->DecrementPage();
				}

				// ページタイトルを減らす
				List<PageTitle^>^ tmp = gcnew List<PageTitle^>(this->_pages);
				this->_pages->Clear();
				for each(PageTitle^ p in tmp) {
					if (p->Index < page) {
						this->_pages->Add(p);
					}
					else if (p->Index > page) {
						this->_pages->Add(gcnew PageTitle(p->Index - 1, p->Title));
					}
				}
			}
		}
	}

	int LauncherCollection::SearchPosition(LauncherPosition^ key) {
		int lf = 0, rt = this->_positions->Count, md = 0;

		while (lf < rt) {
			md = lf + (rt - lf) / 2;

			if (this->_positions[md]->CompareTo(key) < 0) {
				lf = md + 1;
			}
			else {
				rt = md;
			}
		}
		return lf;
	}

	Color LauncherCollection::ConvertToColor(String^ inpstr) {
		unsigned int clr = Convert::ToUInt32(inpstr, 16);
		return Color::FromArgb((clr >> 24) & 0xff, (clr >> 16) & 0xff, (clr >> 8) & 0xff, clr & 0xff);
	}

	String^ LauncherCollection::ConvertToString(Color inpclr) {
		return String::Format(L"{0:X2}{1:X2}{2:X2}{3:X2}", 
					inpclr.A, inpclr.R, inpclr.G, inpclr.B);
	}

	int LauncherCollection::PageCount::get() {
		if (this->_positions->Count > 0) {
			return this->_positions[this->_positions->Count - 1]->Page + 1;
		}
		else {
			return 0;
		}
	}
}
