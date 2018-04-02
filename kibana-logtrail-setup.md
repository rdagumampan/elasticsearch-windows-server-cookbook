LogTrail
https://github.com/sivasamyk/logtrail

Baretail is one of my best-loved tools in taling logs but this has to go when we use ElasticSearch. It's great that someone made great work to support this in Kibana as plugin, hats-off to @sivasamyk

Pre-requisite:
	- We need the exact version of Kibana you're running, in my case I had v6.2.2. To view releases that matches your installation visit this https://github.com/sivasamyk/logtrail/releases
	- We will re-use the log format in the previous demo project HelloworldElk

On new CMD window
```
/> cd elk\kibana-6.2.2-windows-x86_64\bin
/> kibana-plugin install https://github.com/sivasamyk/logtrail/releases/download/v0.1.27/logtrail-6.2.2-0.1.27.zip
```

Configure the settindir/ cd elk\kibana-6.2.2-windows-x86_64\plugins\logtrail
```
/> copy logtrail.json logtrail_backup.json
/> notepad++ logtrail.json
```

```json
#copy full json data and replace the existing
{
  "version" : 1,
  "index_patterns" : [
    {      
      "es": {
        "default_index": "hello-world-elk-*"
      },
      "tail_interval_in_seconds": 10,
      "es_index_time_offset_in_seconds": 0,
      "display_timezone": "local",
      "display_timestamp_format": "MMM DD HH:mm:ss",
      "max_buckets": 500,
      "default_time_range_in_days" : 0,
      "max_hosts": 100,
      "max_events_to_keep_in_viewer": 5000,
      "fields" : {
        "mapping" : {
            "timestamp" : "@timestamp",
            "hostname" : "fields.AppServer",
            "message": "message"
        },
        "message_format": "{{{message}}}"
      },
      "color_mapping" : {
      }
    }  
  ]
}
```

Restart Kibana and wait for few seconds
```
/> nssm restart "Elasticsearch - Kibana 6.2.2"
```

![test](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/blob/master/screenshot-kibana-plugin-logtrail.PNG "")

Watch-out for:
If after configuring plugin, Kibana became inaccessible, its probobly dead.If you have syntax error in the logtrail.json file, it will crash Kibana.To figure out the error, stop Kibana service and run as console from CMD. It will show the error.

References:

