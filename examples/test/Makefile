
CSC=mcs
LOG4NET_DLL=log4net.dll
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
LIB="bin/mono/debug/$(LOG4NET_DLL),bin/mono/debug/$(SMARTIRC_DLL)"
TARGET=test.exe

all: debug release

debug: *.cs
	cp ../../bin/$(LOG4NET_DLL) bin/mono/debug/$(LOG4NET_DLL)
	cp ../../bin/mono/debug/$(SMARTIRC_DLL) bin/mono/debug/$(SMARTIRC_DLL)
	$(CSC) /debug /r:$(LIB) /out:bin/mono/debug/$(TARGET) $^

release: *.cs
	cp ../../bin/$(LOG4NET_DLL) bin/mono/release/$(LOG4NET_DLL)
	cp ../../bin/mono/release/$(SMARTIRC_DLL) bin/mono/release/$(SMARTIRC_DLL)
	$(CSC) /r:$(LIB) /out:bin/mono/release/$(TARGET) $^

clean:
	-rm -f bin/mono/debug/test.exe
