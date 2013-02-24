#!/bin/bash

cd ~/dev/elf
git pull
echo "rake assets:precompile"
bundle exec rake assets:precompile
echo "copying files..."
rsync -rvuz ~/dev/elf/ bholt@geekytidbits.com:web/elf --exclude='.git/' --exclude='log/'
echo "bundle install"
ssh bholt@geekytidbits.com 'cd ~/web/elf && bundle install'
echo "rake tmp:clear"
ssh bholt@geekytidbits.com 'cd ~/web/elf && bundle exec rake tmp:clear'
echo "touch tmp/restart.txt"
ssh bholt@geekytidbits.com 'touch ~/web/elf/tmp/restart.txt'
echo "Deploy Successful!"
