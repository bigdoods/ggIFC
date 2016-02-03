#!/bin/bash

find ./ -iname '*.csproj' | {
	while read -r file_path; do
		sed -ri 's/..\\..\\(ifc|step)\\/..\\..\\\U\1\\/' "$file_path"
	done
}

exit 0