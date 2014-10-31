CONFIG=$1
if [ -z $CONFIG ]; then CONFIG=Debug; fi
rsync -u ../Compilers/bin/$CONFIG/Playroom.Compilers.* bin/$CONFIG/
