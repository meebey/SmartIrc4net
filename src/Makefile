
CSC=mcs

all: SmartIRC\#.dll

SmartIRC\#.dll: *.cs
	$(CSC) /debug /target:library /r:../bin/log4net.dll /out:../bin/mono/debug/$@ $^

clean:
	-rm -f ../bin/mono/debug/SmartIRC#.dll
