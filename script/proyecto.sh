#!/bin/bash
if [[ "$1" == "run" ]]; then
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
      powershell.exe -Command "cd MoogleServer; dotnet watch run"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open -a Terminal.app MoogleServer -n --args bash -c "dotnet watch run"
    elif [[ "$OSTYPE" == "linux-gnu" ]]; then
        gnome-terminal --working-directory="$PWD/MoogleServer" -x bash -c "dotnet watch run"
    fi
    sleep 4 
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        start "http://localhost:5285"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open "http://localhost:5285"
    elif [[ "$OSTYPE" == "linux-gnu" ]]; then
        xdg-open "http://localhost:5285"
    fi
elif [[ "$1" == "show_report" ]]; then
    pdflatex ./informe/Inform.tex
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        start "$(cygpath -w Inform.pdf)"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open Inform.pdf
    elif [[ "$OSTYPE" == "linux-gnu" ]]; then
        xdg-open Inform.pdf
    fi
elif [[ "$1" == "report" ]]; then
    pdflatex ./informe/Inform.tex
    elif [[ "$1" == "show_slides" ]]; then
    pdflatex ./presentación/presentation.tex
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        start "$(cygpath -w presentation.pdf)"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open presentation.pdf
    elif [[ "$OSTYPE" == "linux-gnu" ]]; then
        xdg-open presentation.pdf
    fi
elif [[ "$1" == "show_slides" ]]; then
    pdflatex ./presentación/presentation.tex
    if [[ "$OSTYPE" == "msys" ]] || [[ "$OSTYPE" == "cygwin" ]]; then
        start "$(cygpath -w presentation.pdf)"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        open presentation.pdf
    elif [[ "$OSTYPE" == "linux-gnu" ]]; then
        xdg-open presentation.pdf
    fi
elif [[ "$1" == "slides" ]]; then
    pdflatex ./presentación/presentation.tex
elif [[ "$1" == "clean" ]]; then
    find . -type f -regextype posix-extended -regex '.*\.(aux|bbl|fdb_latexmk|fls|pdf|gz|toc|log|nav|out|snm)' -delete
    find . -name ".vscode" -type d -exec rm -rf {} +
    find . -type d -name "bin" -o -name "obj" -exec rm -rf {} +
fi