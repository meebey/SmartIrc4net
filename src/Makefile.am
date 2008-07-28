TARGET_DIR = $(top_srcdir)/bin
KEYFILE = $(top_srcdir)/$(PACKAGE_NAME).snk
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_TARGET = $(TARGET_DIR)/$(ASSEMBLY)
ASSEMBLY_XML = $(ASSEMBLY_NAME).xml
ASSEMBLY_XML_TARGET = $(TARGET_DIR)/$(ASSEMBLY_XML)
ASSEMBLY_PC = $(top_srcdir)/$(PACKAGE_NAME).pc
NDOC = ndoc-console
NDOC_TARGET_DIR = docs/html
SOURCE_FILES = *.cs */*.cs
GACUTIL_INSTALL = $(GACUTIL) -i $(ASSEMBLY_TARGET) -f $(GACUTIL_FLAGS)
GACUTIL_UNINSTALL = $(GACUTIL) -u $(ASSEMBLY_NAME) $(GACUTIL_FLAGS)

# automake magic variables
EXTRA_DIST = $(SOURCE_FILES)
CLEANFILES = $(ASSEMBLY_TARGET)

pkglib_DATA = $(ASSEMBLY_TARGET)

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = $(ASSEMBLY_PC)

all: $(ASSEMBLY_TARGET)

$(ASSEMBLY_TARGET): $(SOURCE_FILES)
	$(INSTALL) -d $(TARGET_DIR)
	$(CSC) $(CSC_FLAGS) -keyfile:$(KEYFILE) -doc:$(ASSEMBLY_XML_TARGET) -target:library -out:$@ $^

docs: $(ASSEMBLY_TARGET)
	$(NDOC) $(ASSEMBLY) \
	  -documenter=MSDN -OutputTarget=Web -OutputDirectory=$(NDOC_TARGET_DIR) \
	  -Title="SmartIrc4net API documentation" -SdkLinksOnWeb=true \
	  -AssemblyVersionInfo=AssemblyVersion

install-data-local:
	echo "$(GACUTIL_INSTALL)";  \
	  $(GACUTIL_INSTALL)  || exit 1;

uninstall-local:
	echo "$(GACUTIL_UNINSTALL)";  \
	  $(GACUTIL_UNINSTALL)  || exit 1;
