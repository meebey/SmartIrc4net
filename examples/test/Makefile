
CSC=mcs
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
TARGET=test.exe

all: release

debug: TARGET_PATH=bin/mono/debug
debug: *.cs
	mkdir -p $(TARGET_PATH)
	cp ../../$(TARGET_PATH/$(SMARTIRC_DLL) $(TARGET_PATH)/$(SMARTIRC_DLL)
	$(CSC) /debug /r:$(TARGET_PATH)/$(SMARTIRC_DLL) /out:$(TARGET_PATH)/$(TARGET) $^

release: TARGET_PATH=bin/mono/release
release: *.cs
	mkdir -p $(TARGET_PATH)
	cp ../../$(TARGET_PATH)/$(SMARTIRC_DLL) $(TARGET_PATH)/$(SMARTIRC_DLL)
	$(CSC) /r:$(TARGET_PATH)/$(SMARTIRC_DLL) /out:$(TARGET_PATH)/$(TARGET) $^

clean:
	-rm -f bin/mono/debug/$(TARGET)
	-rm -f bin/mono/release/$(TARGET)
