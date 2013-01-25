#! /usr/bin/python
import os
import os.path
import shutil
import glob
import sys

def copyPaths(paths, todir):
	for path in paths:
		parts = os.path.split(path)
		topath = os.path.join(todir, parts[1])
		shutil.copy2(path, topath)
		print path + ' -> ' + topath

rootpath = os.path.abspath(os.path.join(os.path.dirname(sys.argv[0]), '..'))
bindir = os.path.expanduser('~/bin/')
playroomdir = os.path.join(bindir, 'Playroom.app')

if (not os.path.exists(playroomdir)):
	os.makedirs(playroomdir)

patterns = [
	[ os.path.join(rootpath, 'BuildContent/bin/Release/*.dll'), playroomdir ], 
	[ os.path.join(rootpath, 'BuildContent/bin/Release/*.exe'), playroomdir ], 
	[ os.path.join(rootpath, 'Squish2/Squish2/bin/Release/*.so'), playroomdir ], 
	[ os.path.join(rootpath, 'Compilers/bin/Release/*.Compilers.dll'), playroomdir ], 
	[ os.path.join(rootpath, 'Scripts/buildcontent'), bindir ] 
	]

for pattern in patterns:
	copyPaths(glob.glob(pattern[0]), pattern[1])
