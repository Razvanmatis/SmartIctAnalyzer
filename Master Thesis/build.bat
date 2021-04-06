@echo off

del *.pdf

pdflatex.exe Structure.tex
pdflatex.exe Structure.tex

ren Structure.pdf ZFTRW_GUI_Manual.pdf

del /f /q *.out 
del /f /q *.toc 
del /f /q *.aux 
del /f /q *.hst
del /f /q *.ver
del /f /q *.log
del /f /q *.md
del /f /q *.nlo