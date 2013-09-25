#!/bin/bash

cd ~/dev/elf
git pull
echo "rake assets:precompile"
bundle exec rake assets:precompile
echo "copying files..."
rsync -rvuz ~/dev/elf/ bholt@gtb:web/elf --exclude='.git/' --exclude='log/'
echo "bundle install"
ssh bholt@gtb 'cd ~/web/elf && bundle install'
echo "rake tmp:clear"
ssh bholt@gtb 'cd ~/web/elf && bundle exec rake tmp:clear'
echo "touch tmp/restart.txt"
ssh bholt@gtb 'touch ~/web/elf/tmp/restart.txt'
echo "Deploy Successful!"
