// version.h - define WzWXF Provider version numbers and strings
//
// This software is property of Corel Corporation and its licensors and
// is protected by copyright.  Any reproduction in whole or in part is
// strictly prohibited.
//
// (c) 1991-2023 Corel Corporation All rights reserved.

#ifndef VERSION_H
#define VERSION_H

#include "..\Common\WzVersion.h"

#define COPY_RIGHT L"2023"

#define WXF_PRODUCT_VER_STR  WZVERSION L"(" BUILD_VERSION L")"
#define WXF_FILE_VER_STR  WZMAJORSTR L"." WZMINORSTR L"." BUILD_VERSION L".0"

#define COPYRIGHT_STRING L"(c) 2015-" COPY_RIGHT L" Corel Corporation All rights reserved."
#define COMPANY_NAME L"WinZip Computing"
#define TRADEMARKS   L"WinZip is a registered trademark of Corel Corporation"
#define PRODUCT_NAME L"WinZip"

#endif
