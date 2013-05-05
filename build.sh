#!/bin/bash
set -e
cd $( dirname "$0" )

rm -r AutoExecuter/bin AutoExecuter/obj || true

XBUILDARGS="/p:Configuration=Release" # /p:TargetFrameworkProfile=\"\""

xbuild $XBUILDARGS AutoExecuter.sln

