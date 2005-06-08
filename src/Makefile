CSC=mcs
CSS=sn
GACUTIL=gacutil
LOG4NET_DLL=log4net.dll
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
TARGET=$(SMARTIRC_DLL)
DOC_TARGET=Meebey.SmartIrc4net.xml
DEBUG_DEFINES=TRACE,DEBUG,LOG4NET
	
all: release

debug: debug-stamp
debug-stamp: *.cs */*.cs
	$(CSC) /debug /define:$(DEBUG_DEFINES) /target:library /r:../bin/$(LOG4NET_DLL) /out:../bin/mono/debug/$(TARGET) $^
	touch debug-stamp

release: release-stamp
release-stamp: *.cs */*.cs
	$(CSC) /target:library /doc:../bin/mono/release/$(DOC_TARGET) /out:../bin/mono/release/$(TARGET) $^
	touch release-stamp

release-signed: release-signed-stamp
release-signed-stamp: *.cs */*.cs
	$(CSC) /target:library /define:DELAY_SIGN /out:../bin/mono/release/$(TARGET) $^
	$(CSS) -R ../bin/mono/release/$(TARGET) ../SmartIrc4net.snk
	touch release-signed-stamp

install: relase
	$(GACUTIL) -i ../bin/mono/release/$(SMARTIRC_DLL) -f -package SmartIrc4net

clean:
	-rm -f ../bin/mono/debug/$(TARGET)
	-rm -f ../bin/mono/release/$(TARGET)
	-rm -f debug-stamp
	-rm -f release-stamp
	-rm -f release-signed-stamp

.PHONY: all debug release release-signed install clean
