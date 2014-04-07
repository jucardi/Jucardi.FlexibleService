Jucardi.FlexibleService
=======================

What is Jucardi.FlexibleService and how does it work?
-----------------------------------------------------

This is a flexible windows service which allows to run multiple processes on the background with addons.

All the addons work "copy and run", the service has a folder called "Extensions", where all the addons
may be placed. The addons are simply assemblies (dll files), which contain one or more .NET clases which
implement the interface IWorker which is in the core dll called Jucardi.FlexibleService.Common.dll.

The workers are ran automatically when an addon dll is placed in the "Extensions" folder and the workers
contained within are also configured in the Jucardi.FlexibleService.config.xml file, which is also read
dinamically.

The service does not block a handle to any worker dll. The way an assembly is loaded is by reading it's
raw data into the memory and then loading that assembly in a separate AppDomain. The reason this is done
is so if an extension dll is removed from the "Extensions" folder, the AppDomain in which it was executed
is also disposed leaving no trace to the assembly loaded in the execution of the service.

If an extension is updated with a new version, the AppDomain where that assembly execution was been made
is disposed and restarted.

If the configuration file (Jucardi.FlexibleService.config.xml) is modified, that change is also monitored,
so all the workers will be restarted to make sure they run with the new configuration provided.


How to configure a worker?
--------------------------

An entry <add> in the <types> node in the configuration file must be made, specifying the assembly dll and
the worker class and namespace. Inside the <add> node a <properties> node may be placed so all the class
properties may be assigned that way. Here's an example worker configured in the configuration file:

  <types>
    <add name="Any name to identify this worker" assembly="Extensions/Jucardi.SampleWorker.dll" class="Jucardi.SampleWorker.TestWorker">
      <properties>
        <property name="Interval" value="20000" class="System.Int32"/>
      </properties>
    </add>
  </types>

For now, if a worker is not configured in the <types> section, it won't be loaded.


How to install the service?
---------------------------

1) Simply place the following files in a folder of your choosing:
    - Jucardi.FlexibleService.exe
    - Jucardi.FlexibleService.Common.dll
    - log4net.dll
    - Jucardi.FlexibleService.config.xml
    - And the folder Extensions with any worker dll you wish to run.

2) Open a Command Promt with Administrator priviledges.
3) Locate the folder where the service was copied and type Jucardi.FlexibleService.exe /install

The service may be also executed as a console application by typing Jucardi.FlexibleService.exe /console

