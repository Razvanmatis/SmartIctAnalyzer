@echo off

del *.pdf


for %%B in (%~dp0\.) do set c=%%~dpB
for %%a in (%c%\.) do set driver=%%~na

xcopy /S /Y \\samba.promik.local\Development\Documents\Templates\Latex\GuiManual
xcopy /S /Y \\samba.promik.local\Development\Documents\Templates\Latex\Common

if not exist "%cd%\Content.tex" ( type NUL > %cd%\Content.tex )
if not exist "%cd%\Revision.tex" ( type NUL > %cd%\Revision.tex )
if not exist "%cd%\Title.tex" ( type NUL > %cd%\Title.tex )
if not exist "%cd%\Appendix.tex" ( type NUL > %cd%\Appendix.tex )

"C:\Users\ram\AppData\Local\Programs\MiKTeX 2.9\miktex\bin\x64\pdflatex.exe" Structure.tex
"C:\Users\ram\AppData\Local\Programs\MiKTeX 2.9\miktex\bin\x64\pdflatex.exe" Structure.tex

for %%a in ("%currpath%") do set "p_dir=%%~dpa"
echo %p_dir%
for %%a in ("%p_dir%") do set currentfolder=%%~na
echo %currentfolder%


ren Structure.pdf %driver%.pdf

del /f /q *.out 
del /f /q *.toc 
del /f /q *.aux 
del /f /q *.hst
del /f /q *.ver
del /f /q *.log
del /f /q *.md
del /f /q *.nlo

del /f /q Structure.tex
del /f /q Style.tex
rmdir /s /q images