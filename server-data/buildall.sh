#!/bin/bash

# Find all subdirectories containing a .csproj file and run dotnet build in them
for dir in $(find . -type f -name "*.csproj" -exec dirname {} \; | sort -u); do
  echo "Building in $dir..."
  (cd "$dir" && dotnet build)
done