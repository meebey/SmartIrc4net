
CSC=mcs
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
LOG4NET_DLL=log4net.dll
TARGET=test.exe

all: release

debug: TARGET_PATH=../../bin/mono/debug
debug: *.cs
	mkdir -p $(TARGET_PATH)
	cp ../../bin/$(LOG4NET_DLL) $(TARGET_PATH)
	$(CSC) /debug /r:$(TARGET_PATH)/$(SMARTIRC_DLL) /out:$(TARGET_PATH)/$(TARGET) $^

release: TARGET_PATH=../../bin/mono/release
release: *.cs
	mkdir -p $(TARGET_PATH)
	$(CSC) /r:$(TARGET_PATH)/$(SMARTIRC_DLL) /out:$(TARGET_PATH)/$(TARGET) $^
	
debug-full:
	cd ../../src; make debug
	$(MAKE) debug

release-full: release
	cd ../../src; make release
	$(MAKE) release

clean:
	-rm -f ../../bin/mono/debug/$(TARGET)
	-rm -f ../../bin/mono/release/$(TARGET)

.PHONY: all debug debug-full release release-full clean
