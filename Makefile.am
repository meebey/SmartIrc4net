SUBDIRS = \
	src

EXTRA_DIST = \
	$(PACKAGE_NAME).snk \
	SmartIrc4net.csproj \
	API_CHANGE \
	CHANGELOG \
	CREDITS \
	FEATURES \
	LICENSE \
	README \
	TODO

fetch-packages:
	$(NUGET) restore packages.config -PackagesDirectory ./packages