#!/bin/bash
dotnet watch run --project MoogleServer
while getopts ":d" opcion; do
  case $opcion in
    run)
      modo_debug=true
      dotnet watch run --project MoogleServer
      ;;
    \?)
      echo "Opción inválida: -$OPTARG" >&2
      exit 1
      ;;
  esac
done