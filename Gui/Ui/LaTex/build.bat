@echo off 
echo Initializing LaTex.

del *.pdf
del /f /q *.out 
del /f /q *.toc 
del /f /q *.aux 
del /f /q *.hst
del /f /q *.ver
del /f /q *.log
del /f /q *.md
del /f /q *.nlo
del /f /q *.nls
del /f /q *.ist
del /f /q *.ilg
del /f /q *.bbl
del /f /q *.bbg
del /f /q *.blg
del /f /q *-blx.bib
del /f /q *.bcf
del /f /q *.xml

echo Seting LaTex variables.
set FILE_NAME= testCoverage

set sources[0]="Style"
set sources[1]="testCoverage"
set sources[2]="TestCoverageData"
for /L %%a in (0,1,2) do call latexindent %%sources[%%a]%%.tex -o=%%sources[%%a]%%.tex

echo Creating testCoverage report.
pdflatex.exe -interaction batchmode testCoverage.tex
biber testCoverage.bcf
makeindex testCoverage.nlo -s nomencl.ist -o testCoverage.nls
pdflatex.exe -interaction batchmode testCoverage.tex
pdflatex.exe -interaction batchmode testCoverage.tex

echo Cleaning temp files.
del /f /q *.out 
del /f /q *.toc 
del /f /q *.aux 
del /f /q *.hst
del /f /q *.ver
del /f /q *.log
del /f /q *.md
del /f /q *.nlo
del /f /q *.nls
del /f /q *.ist
del /f /q *.ilg
del /f /q *.bbl
del /f /q *.bbg
del /f /q *.blg
del /f /q *-blx.bib
del /f /q *.bcf
del /f /q *.xml