CONFIG=$1
if [ -z $CONFIG ]; then CONFIG=Debug; fi
rsync -u ./BuildContent/bin/$CONFIG/*.dll ~/bin/Playroom.app/ 
rsync -u ./BuildContent/bin/$CONFIG/*.exe ~/bin/Playroom.app/
rsync -u ./Squish2/Squish2/bin/$CONFIG/*.so ~/bin/Playroom.app/
rsync -u ./Compilers/bin/$CONFIG/*.Compilers.dll ~/bin/Playroom.app/
rsync -u ./Scripts/buildcontent ~/bin/

