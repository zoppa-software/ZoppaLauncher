#include "pch.h"
#include "IconPosition.h"

namespace ZoppaShortcutLibrary {

	IconPosition::IconPosition(int page, int row, int column, int index) :
		_page(page), _row(row), _column(column), _index(index)
	{}

	int IconPosition::CompareTo(IconPosition^ other) {
		for (int i = 0; i < 3; ++i) {
			int l = this->GetOrder(i);
			int r = other->GetOrder(i);
			if (l != r) {
				return (l - r);
			}
		}
		return 0;
	}

	int IconPosition::GetOrder(int index) {
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
