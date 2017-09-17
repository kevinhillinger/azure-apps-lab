# Azure Apps Lab

### Lab Scenario
Contoso Events is a SaaS provider with an online service for concerts, sporting and other event ticket sales. They are redesigning their solution for scale with a microservices strategy and want to implement a POC for the path that receives the most traffic: ticket ordering.

### Objective
In this lab, you will construct and test an end-to-end POC of the Ticket Ordering subsystem to demonstrate to Contoso Events a deployment using Azure PaaS. You will leverage Service Fabric, API Management, Function Apps, Web Apps, Cosmos DB and Azure Active Directory B2C. 


# Getting Started

### 1. Download the lab manual
To get started, download the lab manual located in the [Content](https://microsoft-my.sharepoint.com/:f:/p/kehilli/Eg-rrv26QYlBvFIbQTjy7LgB-6c1JX7VYwLPv-kJQWH5vg) area (restricted).

If you have trouble accessing the lab manual, please email [kevin.hillinger@microsoft.com](mailto:kevin.hillinger@microsoft.com)

### 2. Deploy the azure resources
The lab manual will guide you through deploying the azure resources using ARM templates located in [/deploy](https://github.com/kevinhillinger/azure-apps-lab/tree/master/deploy). If you're already familiar with ARM template deployment, click on the deploy button below.

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fkevinhillinger%2Fazure-apps-lab%2Fmaster%2Fdeploy%2F__azuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>