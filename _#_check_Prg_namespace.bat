@echo off
rem Check if console is started fro top and then pause at end
set PAUSER=pause
C:\Work\SVN\altgame\build\bin\CheckConsole.exe
set LINE=%ERRORLEVEL%
if %LINE% GTR 2 (
	set PAUSER=
)
@echo on
findstr /L /S /C:"using Game." Assets\Prg\*.cs
findstr /L /S /C:"using PRG." Assets\Prg\*.cs
%PAUSER%