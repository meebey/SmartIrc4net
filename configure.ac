AC_PREREQ([2.54])
AC_INIT([smartirc4net], [0.4.5.1])
AM_INIT_AUTOMAKE([foreign])
AM_MAINTAINER_MODE

AC_PROG_INSTALL

ASSEMBLY_TITLE="SmartIrc4net"
ASSEMBLY_DESCRIPTION="IRC library for CLI"
ASSEMBLY_NAME="Meebey.SmartIrc4net"
ASSEMBLY_VERSION="0.4.5.0"
AC_SUBST(ASSEMBLY_TITLE)
AC_SUBST(ASSEMBLY_DESCRIPTION)
AC_SUBST(ASSEMBLY_VERSION)
AC_SUBST(ASSEMBLY_NAME)

SHAMROCK_EXPAND_LIBDIR
SHAMROCK_CHECK_MONO_MODULE(1.2.6)
SHAMROCK_FIND_MONO_RUNTIME
AC_SUBST(CLR, $MONO)
SHAMROCK_FIND_MONO_2_0_COMPILER
AC_SUBST(CSC, $MCS)
CSC_FLAGS="-debug -define:TRACE,DEBUG -nowarn:1591"
AC_SUBST(CSC_FLAGS)
SHAMROCK_CHECK_MONO_2_0_GAC_ASSEMBLIES([
	System
])

AC_PATH_PROG(GACUTIL, gacutil)
if test x$GACUTIL = x; then
	AC_MSG_ERROR(You need gacutil)
fi

#PKG_CHECK_MODULES([LOG4NET], [log4net])
                        
GACUTIL_FLAGS='-root $(DESTDIR)$(libdir)'
AC_SUBST(GACUTIL_FLAGS)

AC_ARG_ENABLE(pkg-config,
	AC_HELP_STRING([--disable-pkg-config],
		[Disable pkg-config install [default=NO]]),
	disable_pkg_config=yes, disable_pkg_config=no
)
AM_CONDITIONAL([INSTALL_PKG_CONFIG], test x$disable_pkg_config = xno)

AC_ARG_ENABLE(pkg-lib,
	AC_HELP_STRING([--disable-pkg-lib],
		[Disable library install [default=NO]]),
	disable_pkg_lib=yes, disable_pkg_lib=no
)
AM_CONDITIONAL([INSTALL_PKG_LIB], test x$disable_pkg_lib = xno)

AC_ARG_ENABLE(pkg-gac,
	AC_HELP_STRING([--disable-pkg-gac],
		[Disable GAC install [default=NO]]),
	disable_pkg_gac=yes, disable_pkg_gac=no
)
AM_CONDITIONAL([INSTALL_PKG_GAC], test x$disable_pkg_gac = xno)

AC_CONFIG_FILES([
	Makefile
	smartirc4net.pc
	src/Makefile
	src/AssemblyInfo.cs
])

AC_OUTPUT

AC_MSG_RESULT([
	Configuration summary for $PACKAGE_NAME ($VERSION)
        
	* Installation prefix: $prefix
])
