CSC = mcs
CSS = sn
CLI = mono
GACUTIL = gacutil
LOG4NET_DLL = log4net.dll
SMARTIRC_DLL = Meebey.SmartIrc4net.dll
TARGET = $(SMARTIRC_DLL)
XML_DOC_TARGET = Meebey.SmartIrc4net.xml
DEBUG_DEFINES = TRACE,DEBUG,LOG4NET
#NDOC=$(CLI) ../../ndoc/bin/mono/1.0/NDocConsole.exe
NDOC = ndoc-console
NDOC_TARGET_DIR = docs/html
SOURCE_FILES = src/*.cs src/*/*.cs
all: release

debug: debug-stamp
debug-stamp: $(SOURCE_FILES)
	$(CSC) /debug /define:$(DEBUG_DEFINES) /target:library /r:bin/$(LOG4NET_DLL) /out:bin/debug/$(TARGET) $^
	touch debug-stamp

release: release-stamp
release-stamp: $(SOURCE_FILES)
	$(CSC) /target:library /doc:bin/release/$(XML_DOC_TARGET) /out:bin/release/$(TARGET) $^
	touch release-stamp

release-signed: release-signed-stamp
release-signed-stamp: $(SOURCE_FILES)
	$(CSC) /target:library /define:DELAY_SIGN /out:bin/release/$(TARGET) $^
	$(CSS) -R bin/release/$(TARGET) ../SmartIrc4net.snk
	touch release-signed-stamp

docs: release
	$(NDOC) bin/release/$(SMARTIRC_DLL) \
	  -documenter=MSDN -OutputTarget=Web -OutputDirectory=$(NDOC_TARGET_DIR) \
	  -Title="SmartIrc4net API documentation" -SdkLinksOnWeb=true \
	  -AssemblyVersionInfo=AssemblyVersion

install: release
	$(GACUTIL) -i bin/release/$(SMARTIRC_DLL) -f -package SmartIrc4net

clean:
	-rm -f bin/debug/$(TARGET)
	-rm -f bin/debug/$(TARGET).mdb
	-rm -f bin/release/$(TARGET)
	-rm -f debug-stamp
	-rm -f release-stamp
	-rm -f release-signed-stamp

.PHONY: all debug release release-signed install clean
