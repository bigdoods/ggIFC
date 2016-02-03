# ggIFC

GeometryGym DLL, Rhino plugin and CLI

## Development

### Linux

#### Install mono
```bash
$ aptitude install mono
```

or

```bash
$ yaourt mono
```


#### Automatic Build
```bash
$ make debug # or release
```


#### Manual Build
##### Compatability

There may be some path case iregularities when building from the csproj files, there is a script to make file paths unix compatible

```bash
$ bash scripts/clean_project_files.sh
```


##### Compile

Within the relevant project directory
```bash
$ xbuild ./*.csproj
```

This will create a bin directory within the project directory


#### Run

```bash
$ mono bin/testConsole.exe < filename.ifc
```
