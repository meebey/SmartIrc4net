CSC=mcs
LOG4NET_DLL=log4net.dll
SMARTIRC_DLL=Meebey.SmartIrc4net.dll
LIB="bin/mono/debug/$(LOG4NET_DLL),bin/mono/debug/$(SMARTIRC_DLL)"
TARGET=$(SMARTIRC_DLL)

all: debug release

debug: *.cs
	$(CSC) /debug /target:library /r:../bin/$(LOG4NET_DLL) /out:../bin/mono/debug/$(TARGET) $^

release: *.cs
	$(CSC) /target:library /r:../bin/$(LOG4NET_DLL) /out:../bin/mono/release/$(TARGET) $^

clean:
	-rm -f ../bin/mono/debug/$(TARGET)
	-rm -f ../bin/mono/release/$(TARGET)
