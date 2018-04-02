#### Background

In my team, we have setup principle that we should "keep our hands off" from the application servers. The idea is to manage as much as possible from a automation scripts or GUI to reduce risk of human errors. Each active RDP session also consumes precious RAM and CPU from application servers. And the first thing that drives developers to the server is checking log files. Let's strike that down.

Now, there are many different ways to achieve a cetralized logging infrastructure. 

1. Just create create shared folder on NAS where your logging framework can sink logs.
2. You can sink to any RDMS or NoSQL datastore like SQL Server Express or MongoDB or Redis. 
3. Instrument your code using much more sophisticated commercial tools like Application Insights, AppDynamics, New Relic.
4. Pickup from community like Elasticsearch + Logstash + Kibana (ELK).

While we are evaluating AppInsights and AppDynamics at enterprise level, I can't can't wait before something gets signed. We got to do something as our services grows every sprint. We are moving to ELK.

#### Objectstives
The purpose of this entry is to give you a defintive guide in setting up your Elasticsearch+Logstach+KIbana (ELK) stack on an on-premise servers including my preferred open source plugins and management tools. The guide will not cover securing your nodes and using commercial tools like x-pack, sematext as I think they deserve a entry. 

In summary, we will:
1. Setup ELK stack
2. Setup plugins and management tools
3. Integrate with .NET service
4. Backup log data
5. Clean-up old log data
6. Recommend further reading

Pre-requsites:

- Windows Server 2012+
- Server must have access to the internet to download packages
- You must have Admin privilege in the server

Important:
- Always re-start Command Prompt when you made changes to environment variables
- Choose the version of ES and Kibana (they have to be of the same version. In this artictle, we're using v6.2.2.

Estimate TAT:
- 2 hours

#### Install required runtime and supporting tools

1. Fire-up your windows server
2. Run CMD as Administrator mode
```
/ d:
/ md elk
/ cd elk
/ md _bin
/ md _installers
```
3. Download and install **git** for windows x64
<br>https://git-scm.com/download/win
4. Download and install **Notepad++** x64
<br>https://notepad-plus-plus.org/download/v7.5.6.html
<br>Add Notepad++ path to PATH
`C:\Program Files\Notepad++`
5. Download and install JRE x64
<br>http://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html
<br>Set JAVA_HOME system environment folder
```
/ echo %JAVA_HOME%
```
6. Download and install **NodeJS** x64
<br>https://nodejs.org/en/download/
```
/ node -v
/ npm -v
/ npm install
```
7. Download and install **Python** x86/x64
<br>Choose the Windows MSI installer
<br>https://www.python.org/downloads/
<br>Add Python into PATH
`
C:\Program Files (x86)\Python34
C:\Program Files (x86)\Python34\Scripts
`
```
/ echo %PATH%
```
8. Download Python/pip
<br>https://bootstrap.pypa.io/get-pip.py
<br>Copy file into `C:\Program Files (x86)\Python34\Scripts`

9. Download **ElasticSearch** 6.2.2
<br>https://www.elastic.co/downloads/elasticsearch
<br>Extract files into `C:\elk\`

10. Download **Kibana** 6.2.2
<br>https://www.elastic.co/downloads/kibana
<br>Extract file into `C:\elk\`

11. Download NSSM
<br>NSSM is required to make it so "Logstash" and "Kibana" can run as windows services.
<br>https://nssm.cc/download
<br>Extract the files into 'c:\elk\'
<br.Add NSSM install folder to PATH
```
/ echo %PATH%
```

#### Readiness check

- At this point you should have access to the following commands in your Command Prompt window
```
- git --help
- npm -l
- python --help
- nssm -help
```
- You environment system PATH should be
- Your ELK folder should look like this
- Install ELK stack
- Install Elasticsearch 6.2.2

**Dry-run ES**
```
/ cd elk
/ cd elasticsearch-6.2.2\bin
/ elasticsearch.bat
```

**Host ES as windows service**
```
/ nssm install "Elasticsearch - Core 6.2.2" c:\elk\elasticsearch-6.2.2\bin\elasticsearch.bat
/ nssm set "Elasticsearch - Core 6.2.2" Start "SERVICE_DELAYED_AUTO_START"
/ nssm set "Elasticsearch - Core 6.2.2" Description "Elasticsearch v6.2.2 core services"
/ nssm start "Elasticsearch - Core 6.2.2"
```

It helps to be explicit on the version of ES. This guide's us in determinining compatbility of our plugins and suppporting software.

Run ES (via PostMan)
http://localhost:9200/

Check ES cluster status
http://osi2553:9200/_cat/indices?v

Check ES nodes status
http://osi2553:9200/_nodes?pretty=true

Install Kibana 6.2.2

Dry-run ES
```
/ cd elk
/ cd kibana-6.2.2-windows-x86_64\bin
/ kibana.bat
```

Host Kibana as windows service\
```
/ nssm install "Elasticsearch - Kibana 6.2.2" c:\elk\kibana-6.2.2-windows-x86_64\bin\kibana.bat
/ nssm set "Elasticsearch - Kibana 6.2.2" Start "SERVICE_DELAYED_AUTO_START"
/ nssm set "Elasticsearch - Kibana 6.2.2" Description "Kibana lets you visualize your Elasticsearch data"
/ nssm start "Elasticsearch - Kibana 6.2.2"
```

It helps to be explicit on the version of Kibana. This guide's us in determinining compatbility of our plugins.

Run Kibana (a nodeJS app)
http://localhost:5601/

Install ELK tools and Kibana plugins

Head
https://github.com/mobz/elasticsearch-head

Download and build packages
```
/ cd elk
/ git clone git://github.com/mobz/elasticsearch-head.git
/ cd elasticsearch-head
/ npm install
```

Dry-run service
```
/ npm run start
/ open http://localhost:9100/
```

Allow CORS in ES Core

NOTE: While ES is running, head was not able to connect to ES because CORS request is disabled by default. We need to reconfigure ES allow CORS reqyests.

ES disabled CORS requests by default from version 5.x. To make work with Head,  edit elasticsearch.yml and restart elasticsearch service.
```
/ cd elk
/ cd elasticsearch-6.2.2\config
/ notepad++ elasticsearch.yml
```

	http.cors.enabled: true
	http.cors.allow-origin: "*"

``
/ nssm restart "Elasticsearch - Core 6.2.2"
``

The gree cluster health indicator shows we have successfully paired Head with ES core.This means other plugins may not be able to connect to ES API.

Host as a windows service
Because we using ES v6.x, we have to host Head independent from ES web server. It's pretty simple with NSSM, but first we need to create a bootstrap file.

On new CMD window:
```
/ cd c:\elk\elasticsearch-head
/ copy NUL RunMe.bat
/ notepad++ Runme.bat
```

#Put this code into RunMe.bat and save
```
cd /d %~dp0
npm start
```

On new CMD window:
```
/ cd c:\elk\elasticsearch-head
/ nssm install "Elasticsearch - Head" c:\elk\elasticsearch-head\runme.bat
/ nssm set "Elasticsearch - Head" Start "SERVICE_DELAYED_AUTO_START"
/ nssm set "Elasticsearch - Head" Description "Head plugin for Elasticsearch"
/ nssm start "Elasticsearch - Head"
```

Elastic HQ
http://www.elastichq.org/gettingstarted.html

Download and build packages
On new CMD window:
```
/ cd elk
/ git clone https://github.com/ElasticHQ/elasticsearch-HQ.git
/ cd elasticsearch-hq
/ npm install
```

Get the latest version of PIP
Download and place this file on C:\Program Files (x86)\Python 34\Tools\Scripts
https://bootstrap.pypa.io/get-pip.py

```
/ python C:\Program Files (x86)\Python 34\Tools\Scripts\get-pip.py
/ pip install -r requirements.txt
```

Dry-run service
```
/ python application.py
```

http://localhost:5000/
http://localhost:5000/api

Host as a windows service

On new CMD window:
```
/ cd c:\elk\elasticsearch-hq
/ copy NUL RunMe.bat
/ notepad++ Runme.bat
```

#Put this code into RunMe.bat and save
```
cd /d %~dp0
python application.py
```

```
/ cd <elasticsearch-hq-folder>
/ nssm install "Elasticsearch - HQ" c:\elk\elasticsearch-hq\runme.bat
/ nssm set "Elasticsearch - HQ" Start "SERVICE_DELAYED_AUTO_START"
/ nssm set "Elasticsearch - HQ" Description "Elasticsearch-HQ plugin for Elasticsearch"
/ nssm start "Elasticsearch - HQ"
```

#### Challenges

- Since ES has removed direct suppport for plugins from 5.x, it has been lotmore difficult to setup plugins for ES. We are given two choices, to configure the plugin as native to Kibana or self-host in a web server.

- ELK tools are very much dependent to the version of ES. This means we need to always keep in an eye to compatibility matrix to make sure we dont' break things when we upgrade ES or the plugins we use.

- Several open tools requires that we build the packages ourself. Because they are built from different tools, we have to prepare and install their dependencies like python, pip, setuptools, cx_Freeze etc...

#### FAQ

#### Next Steps

#### Feedback
Twitter: @rdagumampan
Email: rdagumampan|AT|gmail.com

#### References
Plugins
https://www.elastic.co/guide/en/kibana/current/known-plugins.html

Management and monitoring
https://signalfx.com/blog/how-we-monitor-and-run-elasticsearch-at-scale/
https://medium.com/@dionnis/elasticsearch-monitoring-and-maintenance-tools-research-18c5fb45a747

Elasticeach metrics to watch
https://www.oreilly.com/ideas/10-elasticsearch-metrics-to-watch

http://ikeptwalking.com/structured-logging-using-serilog/
https://bitnami.com/stack/elk/installer
https://logz.io/learn/complete-guide-elk-stack/
https://qbox.io/blog/how-to-elasticsearch-logstash-kibana-manage-logs

https://code972.com/blog/2016/04/97-the-definitive-guide-for-elasticsearch-2-x-on-microsoft-azure
