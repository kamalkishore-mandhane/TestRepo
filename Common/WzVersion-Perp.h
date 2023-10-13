// WzVersion-Perp.h - define version numbers and strings for PERPETUAL release
//
// This software is property of Corel Corporation and its licensors and
// is protected by copyright.  Any reproduction in whole or in part is
// strictly prohibited.

// PRODUCT: WinZip(R)
// BETADEF:
// (c) 1991-2023 Corel Corporation All rights reserved.
// RIGHTS: All rights reserved.

// If BUILD_NUMBER_ONLY is defined, this file only defines the build number.
// This is useful for components that major/minor versions that are different
// from the WinZip version but still use the daily build number.

#ifndef WZVERSION_PERP_H
#define WZVERSION_PERP_H

#if !defined(BUILD_NUMBER_ONLY)
#define WZMAJORVER     28
#define WZMAJORSTR     "28"
#define WZMINORVER     0
#define WZMINORSTR     "0"
#define WZSUFFIXSTR    ""      // For things like 'SR-1' - include a leading space
#define WZVERSION WZMAJORSTR "." WZMINORSTR WZSUFFIXSTR
#endif

#define BUILD_NUMBER   15660   // format: (year - 1997) * 1000 + julian date + (365 * ~((year-1997) & 1))
#define BUILD_VERSION "15660"  // format: (year - 1997) * 1000 + julian date + (365 * ~((year-1997) & 1))

#endif
