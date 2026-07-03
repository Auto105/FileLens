@echo off
title FileLens - Codex

cd /d "%~dp0"

echo ==========================================
echo FileLens Development Environment
echo ==========================================
echo.

git branch --show-current
git status --short

echo.
echo Starting Codex...
echo.

"C:\Users\Auto\AppData\Local\Programs\OpenAI\Codex\bin\codex.exe" ^
  -s workspace-write ^
  -a on-request