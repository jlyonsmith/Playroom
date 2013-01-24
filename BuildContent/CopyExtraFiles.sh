CONFIG=$1
if [ -z $CONFIG ]; then CONFIG=Debug; fi
rsync -u ../Squish2/Squish2/bin/$CONFIG/libSquish2.so bin/$CONFIG/
rsync -u ../Compilers/bin/$CONFIG/Playroom.Compilers.* bin/$CONFIG/
rsync -u /Developer/MonoTouch/usr/lib/mono/2.1/System.Json.dll bin/$CONFIG/
