Step-by-step guide in setting Azure VM for hosting ElasticSearch
-----

1. Edit elasticsearch.yml
```
network.host: 0.0.0.0
http.port: 9200
```

2. Edit kibana.yml
```
server.host: 0.0.0.0
server.port: 5601
```

3.InBound ports to open in Network Security Group
9200 es
9100 head
6501 kibana
5000 hq

**References**
https://stackoverflow.com/questions/29332543/elasticsearch-installed-on-azure-linux-vm-isntt-reachable?rq=1
