cd /var/www/html/ellaleeforest.org/
sudo svn update
sudo rake assets:precompile
sudo touch tmp/restart.txt