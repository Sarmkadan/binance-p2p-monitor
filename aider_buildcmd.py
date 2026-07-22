#!/usr/bin/env python3
"""
Simple build helper script.

Running this script will execute `dotnet test` in the repository root,
allowing you to build and run all unit tests with a single command.

Usage:
    python3 ./aider_buildcmd.py
"""

import subprocess
import sys
from pathlib import Path

def main() -> int:
    repo_root = Path(__file__).parent.resolve()
    # Ensure we are in the repository root where the solution / test projects reside.
    try:
        result = subprocess.run(
            ["dotnet", "test"],
            cwd=repo_root,
            check=False,
        )
        return result.returncode
    except FileNotFoundError:
        print("Error: 'dotnet' executable not found. Make sure the .NET SDK is installed and on PATH.", file=sys.stderr)
        return 1

if __name__ == "__main__":
    sys.exit(main())
