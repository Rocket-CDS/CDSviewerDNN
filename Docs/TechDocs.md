# CDSviewerDNN
###### Techical Documentation  

### Introduction

CDSviwer is an extension to display and use html/data from a **Centeral Data Service** on your DNN website.  
This document will outline the technical explaination of how it works.

### Installation

The CDSviewer is installed the same as all DNN modules.  **[Settings>Extensions>Install Extension]**  Once installed the CDSviewer module will be available when you add a new module.

### Central Data Services

Central Data Services (CDS) are online services that provider UI and API interface to display and work with data.  This can range from a simple html text display to a full e-commerce system.  

The advantages are many, but the main concept is to seperate the data from the website or display system (like a mobile app).  

These are often called headless systems, and are not new.  But with new technology and improvments in API methods, a true CDS system can be truly integrated seamlessly into a website or mobile app.  With the CDSviwer it's impossible to see that the data comes from a CDS.

This gives the CDSviewer the ability to morph into any type of module required.  CDSviewer can be a HTMNL text display, a hero banner, a catalog system, a map even an e-commerce.  The power is in which CDS is being used.  

The CDS can be an external service, hosted on a different server.  Or even a CDS hosted on the same DNN installation.  
Any CDS can be used mulitple times on multiple devices.  Therefore data entered into a CDS can have very powerful usage.  Website migration and design upgrade becomes a low risk and speedy process.  The same data can be used to create mulitple websites, with different design but the same data.  Data edited once will automatically update all media devices that use that data.

### How CDSviewer works

The CDSviewer is a server to server API tool, that can display data onto a DNN website.  
The CDSviewer module does NOT have any code to deal with data entry, all functionality for data is dealt with by the CDS.  A CDSviewer is simply an API link to view the data, for both display and edit.  
To create an API link we need to have certain settings,  those settings link this module to the CDS.  
This is the only data that the CDSviewer needs.  To register the CDS the settings data is passed to the CDSviewer as a encoded string, which is simply copied and pasted into the "Data Services" settings of the CDSviewer.  
Once a CDS has been registered it is available for any CDSviewer module on the website.  Multiple CDS can be used on a single website.

### API interface

**Server to Server** interaction is done using the generic RocketComm.dll project.  So CDSviewer is compatible with any CDS which uses the open source RocketCDS.
 
For **Client to Server** interaction CDSviewer uses the RocketCDS standard of simplisity.js.  All Client to Server interaction is to the CDSviewer API, this is to avoid any problems with CORS.  Once CDSviewer receieves the client request it forwards it to the required CDS, which returns data that CDSviewer will display. 



