#!/bin/bash
function evil_git_dirty {
  [[ $(git diff --shortstat 2> /dev/null | tail -n1) != "" ]] && echo "*"
}

APPNAME=HotDot
SCRIPTDIR=$(cd $(dirname $0); pwd -P)
SLNDIR=$(dirname $(${SCRIPTDIR}/upfind.sh $APPNAME.sln))
pushd $SLNDIR
if [[ $(evil_git_dirty) == "*" ]]; then {
    echo "error: You have uncommitted changes! Please stash or commit them first."
    exit 1
}; fi
vamper -u
git add . 
mkdir scratch 2> /dev/null
git commit -F Scratch/$APPNAME.version.txt
popd