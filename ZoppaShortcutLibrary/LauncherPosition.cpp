#include "pch.h"
#include "LauncherPosition.h"

namespace ZoppaShortcutLibrary {

	LauncherPosition::LauncherPosition(int page, int row, int column, int index) :
		_page(page), _row(row), _column(column), _index(index)
	{}

	int LauncherPosition::CompareTo(LauncherPosition^ other) {
		for (int i = 0; i < 3; ++i) {
			int l = this->GetOrder(i);
			int r = other->GetOrder(i);
			if (l != r) {
				return (l - r);
			}
		}
		return 0;
	}

	int LauncherPosition::GetOrder(int index) {
		switch (index)
		{
		case 0:
			return this->_page;

		case 1:
			return this->_row;

		case 2:
			return this->_column;

		default:
			return -1;
		}
	}
}
