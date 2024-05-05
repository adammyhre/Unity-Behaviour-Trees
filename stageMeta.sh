#!/bin/bash

# Stage all Unity meta files for files already staged

# Get the list of staged files
staged_files=$(git diff --name-only --cached)

# Loop through each staged file
while IFS= read -r file; do
    # Check if the file has a corresponding meta file
    meta_file="${file}.meta"
    if [[ -f "$meta_file" ]]; then
        # Stage the meta file if it exists
        git add "$meta_file"
    fi
done <<< "$staged_files"

