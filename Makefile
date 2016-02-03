.PHONY: debug release clean

XBUILD := xbuild
CLEAN := bash scripts/clean_project_files.sh
PROJECT_PATH := Projects/testConsole/testConsole.csproj

XBUILDFLAGS := /p:TargetFrameworkProfile=''
XBUILDFLAGS_DEBUG := $(XBUILDFLAGS) /p:Configuration=Debug
XBUILDFLAGS_RELEASE := $(XBUILDFLAGS) /p:Configuration=Release

debug:
	$(CLEAN)
	$(XBUILD) $(XBUILDFLAGS_DEBUG) $(PROJECT_PATH)

release:
	$(CLEAN)
	$(XBUILD) $(XBUILDFLAGS_RELEASE) $(PROJECT_PATH)

clean:
	$(XBUILD) $(PROJECT_PATH) /t:Clean