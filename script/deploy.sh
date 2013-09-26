#!/bin/bash

cd ~/dev/ellaleeforest
git pull
echo "rake assets:precompile"
bundle exec rake assets:precompile RAILS_ENV=production
echo "copying files..."
rsync -rvuz --delete ~/dev/ellaleeforest/ bholt@gtb:web/elf --exclude='.git/' --exclude='log/'
echo "bundle install"
ssh bholt@gtb 'cd ~/web/elf && bundle install'
echo "touch tmp/restart.txt"
ssh bholt@gtb 'touch ~/web/elf/tmp/restart.txt'
echo "Deploy Successful!"
