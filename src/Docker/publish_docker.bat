rem docker build -t "httpservermock" -f "Dockerfile.alpine" .

docker run -d -p 8888:80 --name httpservermock1 httpservermock