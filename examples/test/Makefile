
CSC=mcs

all: test.exe

test.exe: *.cs
	cp ../../bin/log4net.dll bin/mono/debug/
	cp ../../bin/mono/debug/SmartIRC\#.dll bin/mono/debug/
	$(CSC) /debug /r:../../bin/log4net.dll,../../bin/mono/debug/SmartIRC\#.dll /out:bin/mono/debug/$@ $^

clean:
	-rm -f bin/mono/debug/test.exe
