#!/bin/bash

cd ~/dev/bude_server
git pull
xbuild /p:Configuration="Release"
rsync -rvuz ~/dev/bude_server/server/bin/Release/  bude@geekytidbits.com:server
ssh -t -f bude@geekytidbits.com "sudo service buded restart"
echo "Deploy Success!"
