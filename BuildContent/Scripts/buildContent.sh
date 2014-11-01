#!/bin/bash
#
# A script to find the newest version of a tool in the NuGet packages directory
#

PKGNAME=Playroom
TOOLNAME=BuildContent
SCRIPTDIR=$(cd $(dirname $0); pwd -P)
SLNDIR=$(dirname $(${SCRIPTDIR}/upfind.sh \*.sln))
PKGDIR=${SLNDIR}/packages

# See http://stackoverflow.com/questions/4493205/unix-sort-of-version-numbers

mono $(find $PKGDIR -name $PKGNAME\.\* -type d | sed -Ee 's/^(.*-)([0-9.]+)(\.ime)$/\2.-1 \1\2\3/' | sort -t. -n -r -k1,1 -k2,2 -k3,3 -k4,4 | head -1)/tools/$TOOLNAME.exe $*
