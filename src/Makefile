CSC=mcs
GACUTIL=gacutil
LOG4NET_DLL=log4net.dll
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
TARGET=$(SMARTIRC_DLL)
DEBUG_DEFINES=TRACE,DEBUG,LOG4NET
	
all: release

debug: *.cs */*.cs
	$(CSC) /debug /define:$(DEBUG_DEFINES) /target:library /r:../bin/$(LOG4NET_DLL) /out:../bin/mono/debug/$(TARGET) $^

release: *.cs */*.cs
	$(CSC) /target:library /out:../bin/mono/release/$(TARGET) $^

sign: *.cs */*.cs
	$(CSC) /target:library /define:SIGN /out:../bin/mono/release/$(TARGET) $^

install: release
	$(GACUTIL) -i ../bin/mono/release/$(SMARTIRC_DLL) -f -package SmartIrc4net

clean:
	-rm -f ../bin/mono/debug/$(TARGET)
	-rm -f ../bin/mono/release/$(TARGET)

.PHONY: all debug release install clean
