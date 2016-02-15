@ECHO off

CLS

ECHO ***************************************************
ECHO       Starting Batch File For Data Insertion
ECHO ***************************************************
ECHO.

REM - Insert test data
ECHO Inserting Test Data...
sql aspnetdb < Import.sql
ECHO done
ECHO.


ECHO ***************************************************
ECHO                Script Completed
ECHO ***************************************************
ECHO.

PAUSE

@ECHO on
