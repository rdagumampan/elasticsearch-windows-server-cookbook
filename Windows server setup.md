#### Background

In my team, we have setup principle that we should "keep our hands off" from the application servers. The idea is to manage as much as possible from a automation scripts or GUI to reduce risk of human errors. Each active RDP session also consumes precious RAM and CPU from application servers. And the first thing that drives developers to the server is checking log files. Let's strike that down.

Now, there are many different ways to achieve a cetralized logging infrastructure. 

	1. Just create create shared folder on NAS where your logging framework can sink logs.
	2. You can sink to any RDMS or NoSQL datastore like SQL Server Express or MongoDB or Redis. 
	3. Instrument your code using much more sophisticated commercial tools like Application Insights, AppDynamics, New Relic.
	4. Pickup from community like Elasticsearch + Logstash + Kibana (ELK).

While we are evaluating AppInsights and AppDynamics at enterprise level, I can't can't wait before something gets signed. We got to do something as our services grows every sprint. We are moving to ELK.

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
Get a clean machine up and running
Run cmd as Administrator
/ d:
/ md elk
/ cd elk
/ md _bin
/ md _installers

Download git for windows x64
https://git-scm.com/download/win

#### Install ELK Stack

#### Install ELK Tools and Plugins


#### References