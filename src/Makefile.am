TARGET_DIR = $(top_builddir)/bin
KEYFILE = $(top_srcdir)/$(PACKAGE_NAME).snk
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_TARGET = $(TARGET_DIR)/$(ASSEMBLY)
ASSEMBLY_XML = $(ASSEMBLY_NAME).xml
ASSEMBLY_XML_TARGET = $(TARGET_DIR)/$(ASSEMBLY_XML)
ASSEMBLY_PC = $(top_srcdir)/$(PACKAGE_NAME).pc
NDOC = ndoc-console
NDOC_TARGET_DIR = docs/html
SOURCE_PATTERNS = *.cs */*.cs */*/*.cs
SOURCE_FILES = $(foreach pattern, $(SOURCE_PATTERNS), $(wildcard $(srcdir)/$(pattern)))
GACUTIL_INSTALL = $(GACUTIL) -i $(ASSEMBLY_TARGET) -f $(GACUTIL_FLAGS)
GACUTIL_UNINSTALL = $(GACUTIL) -u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)

# automake magic variables
EXTRA_DIST = $(SOURCE_FILES)
CLEANFILES = $(ASSEMBLY_TARGET) $(ASSEMBLY_TARGET).mdb $(ASSEMBLY_XML_TARGET)

if INSTALL_PKG_LIB
pkgappdir = $(pkglibdir)
pkgapp_DATA = $(ASSEMBLY_TARGET)
endif

if INSTALL_PKG_CONFIG
pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = $(ASSEMBLY_PC)
endif
# end of magic

all: $(ASSEMBLY_TARGET)

$(ASSEMBLY_TARGET): $(SOURCE_FILES)
	$(INSTALL) -d $(TARGET_DIR)
	$(CSC) $(CSC_FLAGS) -keyfile:$(KEYFILE) -doc:$(ASSEMBLY_XML_TARGET) -target:library -out:$@ $^

docs: $(ASSEMBLY_TARGET)
	$(NDOC) $(ASSEMBLY) \
	  -documenter=MSDN -OutputTarget=Web -OutputDirectory=$(NDOC_TARGET_DIR) \
	  -Title="SmartIrc4net API documentation" -SdkLinksOnWeb=true \
	  -AssemblyVersionInfo=AssemblyVersion

if INSTALL_PKG_GAC
install-data-local:
	echo "$(GACUTIL_INSTALL)";  \
	  $(GACUTIL_INSTALL)  || exit 1;

uninstall-local:
	echo "$(GACUTIL_UNINSTALL)";  \
	  $(GACUTIL_UNINSTALL)  || exit 1;
endif

# HACK: autofoo stubbornly creates the $pkglibdir/$package directory
if !INSTALL_PKG_LIB
installdirs:
install-pkglibDATA:
endif
