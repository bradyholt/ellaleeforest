#!/bin/bash

cd ~/dev/bude/
git pull
xbuild /p:Configuration="Release"
rsync -rvuz ~/dev/bude/server/bin/Release/  bude@geekytidbits.com:server
ssh -t bude@geekytidbits.com "sudo service buded restart"
echo "Deploy Success!"
