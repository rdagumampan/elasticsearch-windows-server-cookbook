Background
------

In my team, we have setup principle that we should "keep our hands off" from the application servers. The idea is to manage as much as possible from a automation scripts or GUI to reduce risk of human errors. Each active RDP session also consumes precious RAM and CPU from application servers. And the first thing that drives developers to the server is checking log files. Let's strike that down.

Now, there are many different ways to achieve a cetralized logging infrastructure. 

1. Just create create shared folder on NAS where your logging framework can sink logs.
2. You can sink to any RDMS or NoSQL datastore like SQL Server Express or MongoDB or Redis. 
3. Instrument your code using much more sophisticated commercial tools like Application Insights, AppDynamics, New Relic.
4. Pickup from community like Elasticsearch + Logstash + Kibana (ELK).

While we are evaluating AppInsights and AppDynamics at enterprise level, I can't wait before something gets signed. We got to do something as our services grows every sprint and it's disturbance managing logs from several servers. We are moving to ELK.

Objectives
------

The purpose of this entry is to give you a defintive guide in setting up your Elasticsearch+Logstach+KIbana (ELK) stack on an on-premise servers including my preferred open source plugins and management tools. This guide does not cover securing your nodes and using commercial tools like x-pack, sematext as I think they deserve another entry. 

In summary, we will:
1. Setup an ELK stack on on-premise server
2. Setup management tools and kibana plugins (Head, HQ, curator, esix)
3. Dry-run with small C#/.NET application/service
4. Backup log data (WIP)
5. Clean-up old log data (WIP)
6. Recommend further reading

Pre-requsites:

- Windows Server 2012+
- The server must have access to the internet to download packages
- You must have Admin privilege in the server

Important:
- Always re-start Command Prompt (CMD) after changing the system environment variables
- Choose your version of ES and Kibana. They must have the same version. In this guide, we're using v6.2.2.

Estimate TAT:
- 2 hours

Install required runtime and supporting tools
------

1. Fire-up your windows server
I tested this guide on an Azure Standard D3 VM (4 vcpus, 14 GB memory).

2. Run CMD in Administrator mode
```
/> d:
/> md elk
/> cd elk
/> md _bin
/> md _installers
```

3. Download and install **git for windows** x64
	- Packages from https://git-scm.com/download/win
	- Add git path to PATH system environment variable

4. Download and install **Notepad++** x64
	- Packages from https://notepad-plus-plus.org/download/v7.5.6.html
	- Add Notepad++ path to PATH system environment variable

5. Download and install **JRE** x64
	- Packages from http://www.oracle.com/technetwork/java/javase/downloads/jre8-downloads-2133155.html
	- Set JAVA_HOME system environment variable
	```
	/> echo %JAVA_HOME%
	```
6. Download and install **NodeJS** x64
	- Packages from https://nodejs.org/en/download/
	```
	/> node -v
	/> npm -v
	/> npm install
	```
7. Download and install **Python** x86/x64
	- Choose the Windows MSI installer
	- Packages from https://www.python.org/downloads/
	- Add Python path into PATH system environment variable
	`
	C:\Program Files (x86)\Python34
	C:\Program Files (x86)\Python34\Scripts
	`
	```
	/> echo %PATH%
	```
8. Download **Python/pip**
	- Packages from https://bootstrap.pypa.io/get-pip.py
	- Copy file into `C:\Program Files (x86)\Python34\Scripts`

9. Download **ElasticSearch 6.2.2**
	- Packages from https://www.elastic.co/downloads/elasticsearch
	- Extract files into `C:\elk\`

10. Download **Kibana 6.2.2**
	- Packages from https://www.elastic.co/downloads/kibana
	- Extract file into `C:\elk\`

11. Download NSSM
NSSM is required to make it so "Logstash" and "Kibana" can run as windows services.
	- Packages from https://nssm.cc/download
	- Extract the files into `\elk\`
	- Add NSSM path to PATH system environment variable
	```
	/> echo %PATH%
	```

**Readiness check**
- At this point you should have access to the following commands in your CMD window
```
/> git --help
/> npm -l
/> python --help
/> nssm -help
```
- You environment system PATH should have `git`, `Notepad++`, `NodeJS`, `Phyton` and `NSSM` paths 
- Your ELK folder should look like this

Install ELK stack
------

#### Install Elasticsearch 6.2.2
* __Dry-run ES__
	- On new CMD window:
	```
	/> cd elk
	/> cd elasticsearch-6.2.2\bin
	/> elasticsearch.bat
	```
	- On your browse, open `http://localhost:9200/`
	- Terminate CMD window

* __Host ES as windows service__
	- On new CMD window:
	```
	/> nssm install "Elasticsearch - Core 6.2.2" c:\elk\elasticsearch-6.2.2\bin\elasticsearch.bat
	/> nssm set "Elasticsearch - Core 6.2.2" Start "SERVICE_DELAYED_AUTO_START"
	/> nssm set "Elasticsearch - Core 6.2.2" Description "Elasticsearch v6.2.2 core services"
	/> nssm start "Elasticsearch - Core 6.2.2"
	```
	- On your browser, open `http://localhost:9200/`
	- Check ES cluster status `http://localhost:9200/_cat/indices?v`
	- Check ES nodes status `http://localhost:9200/_nodes?pretty=true`
	- Terminate CMD window

	**NOTE:** It helps to be explicit on the version of ES. This guide's us in determinining compatibility of our plugins and suppporting software.

#### Install Kibana 6.2.2

* __Dry-run Kibana__
	- On new CMD window:
	```
	/> cd elk
	/> cd kibana-6.2.2-windows-x86_64\bin
	/> kibana.bat
	```
	- On your browser, open `http://localhost:5601/`
	- Terminate CMD window

* __Host Kibana as windows service__
	- On new CMD window:
	```
	/> nssm install "Elasticsearch - Kibana 6.2.2" c:\elk\kibana-6.2.2-windows-x86_64\bin\kibana.bat
	/> nssm set "Elasticsearch - Kibana 6.2.2" Start "SERVICE_DELAYED_AUTO_START"
	/> nssm set "Elasticsearch - Kibana 6.2.2" Description "Kibana lets you visualize your Elasticsearch data"
	/> nssm start "Elasticsearch - Kibana 6.2.2"
	```
	- On your browser, open `http://localhost:5601/`
	- Terminate CMD window

	**NOTE:** It helps to be explicit on the version of Kibana. This guide's us in determinining compatibility of our plugins.

Install ELK tools and Kibana plugins
------
Since v5.x, ES have have stopped supporting site plugins in its core installation. Site plugins are those with HTML+CSS files. According to Elastic, this is to protect ES from attacks and vulnerabilities that may be carried by the plugins. This means we have host our web plugins independently and it's pretty simple with NSSM. Bt first, we need to create a bootstrap file for each plugin.

#### Head
Head is your old school GUI for ES. It's simple and it just works for most of my tasks. ES is REST-based, so technically you can do pretty much everything with Head. For details visit https://github.com/mobz/elasticsearch-head.

* __Download and build packages__
	- On new CMD window:
	```
	/> cd elk
	/> git clone git://github.com/mobz/elasticsearch-head.git
	/> cd elasticsearch-head
	/ npm install
	```	
	- Terminate CMD window
* __Dry-run service__
	- On new CMD window:
	```
	/> npm run start
	```
	- On your browser, open `http://localhost:9100/`
	- Terminate CMD window
* __Allow CORS in ES Core__
While ES is running, Head was not able to connect to ES because CORS request is disabled by default. We need to reconfigure ES allow CORS. To fix this, edit `elasticsearch.yml` and restart ES service.

	- On new CMD window:
	```
	/> cd elk
	/> cd elasticsearch-6.2.2\config
	/> notepad++ elasticsearch.yml
	```
	- Put code below into `elasticsearch.yml` file	
	```
		http.cors.enabled: true
		http.cors.allow-origin: "*"
	``
	- Restart ES service
	/> nssm restart "Elasticsearch - Core 6.2.2"
	``
	- Terminate CMD window

The green cluster health indicator shows we have successfully paired Head with ES core.This means other plugins may not be able to connect to ES API.

* __Host as a windows service__
	- On new CMD window:
	```
	/> cd c:\elk\elasticsearch-head
	/> copy NUL RunMe.bat
	/> notepad++ Runme.bat
	```
	- Put this code into RunMe.bat
	```
	cd /d %~dp0
	npm start
	```
	- On new CMD window:
	```
	/> cd c:\elk\elasticsearch-head
	/> nssm install "Elasticsearch - Head" c:\elk\elasticsearch-head\runme.bat
	/> nssm set "Elasticsearch - Head" Start "SERVICE_DELAYED_AUTO_START"
	/> nssm set "Elasticsearch - Head" Description "Head plugin for Elasticsearch"
	/> nssm start "Elasticsearch - Head"
	```
	- Terminate CMD window

#### Elastic HQ

* __Download and build packages__
	- On new CMD window:
	```
	/> cd elk
	/> git clone https://github.com/ElasticHQ/elasticsearch-HQ.git
	/> cd elasticsearch-hq
	/> npm install
	```
	- Get the latest version of PIP
	<br>Download and place this file on `C:\Program Files (x86)\Python 34\Tools\Scripts`
	<br>https://bootstrap.pypa.io/get-pip.py

	- On new CMD window:
	```
	/> python C:\Program Files (x86)\Python 34\Tools\Scripts\get-pip.py
	/> pip install -r requirements.txt
	```
* __Dry-run service__
	- On new CMD window:
	```
	/> python application.py
	```
	- On your browser, open `http://localhost:5000/`
	- Terminate CMD window
* __Host as a windows service__
	- On new CMD window:
	```
	/> cd c:\elk\elasticsearch-hq
	/> copy NUL RunMe.bat
	/> notepad++ Runme.bat
	```
	- Put this code into RunMe.bat
	```
	cd /d %~dp0
	python application.py
	```
	- On new CMD window:
	```
	/> cd <elasticsearch-hq-folder>
	/> nssm install "Elasticsearch - HQ" c:\elk\elasticsearch-hq\runme.bat
	/> nssm set "Elasticsearch - HQ" Start "SERVICE_DELAYED_AUTO_START"
	/> nssm set "Elasticsearch - HQ" Description "Elasticsearch-HQ plugin for Elasticsearch"
	/> nssm start "Elasticsearch - HQ"
	```
	- Terminate CMD window

Noted Challenges
------

- Since ES has removed direct suppport for we plugins from 5.x, it has taken little more effort to setup plugins. We are given two choices: setup plugin as native to Kibana or self-host in a web server.

- ELK tools are very much dependent on the version of ES. This means we need to always keep an eye on the compatibility matrix to make sure we dont' break things when we upgrade ES or the plugins we use.

- Several open tools requires that we build the packages ourself. Because they are built from different tools, we have to prepare and install their dependencies like python, pip, setuptools, cx_Freeze etc...

Next Steps
------

- Verify setup with demo microservice
- Tail logs with **LogTrail**
- Maintaining indices with **curator** or my pet project **esix**

Feedback
------
- Twit me on: [@rdagumampan](https://twitter.com/rdagumampan)
- Drop a mail: rdagumampan|AT|gmail.com
- Or [create an issue](https://github.com/rdagumampan/elasticsearch-windows-server-cookbook/issues) here, I'll try to response as soon as I can

References
------
https://www.elastic.co/blog/running-site-plugins-with-elasticsearch-5-0

Kibana Plugins
<br>https://www.elastic.co/guide/en/kibana/current/known-plugins.html

Management and monitoring
<br>https://signalfx.com/blog/how-we-monitor-and-run-elasticsearch-at-scale/
<br>https://medium.com/@dionnis/elasticsearch-monitoring-and-maintenance-tools-research-18c5fb45a747

Elasticeach metrics to watch
<br>https://www.oreilly.com/ideas/10-elasticsearch-metrics-to-watch

v0.1.0
