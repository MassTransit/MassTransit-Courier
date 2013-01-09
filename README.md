MassTransit-Courier
==================

An implementation of the routing slip pattern (EIP: Routing Slip), including activity execution and compensation.


Routing Slip
------------

A routing slip contains a tracking number, an itinerary, and an activity log. The tracking number is used to uniquely
identify the routing slip as it moves through the system.

Itinerary
---------

The itinerary is a list of activities to be executed. Activites are executed in order. Once completed, an event is published
that the routing slip was ran to completion.


Activities
----------

An activity is code that is executed when a routing slip is received. This could include an inventory check, an authorization
step, or anything other processing. An activity can also be compensated using the log information saved by the activity when executed.


Activity Log
------------

The activity log contains log entries of activites that have been executed. The log is used to allow activites to be
compensated, and a compensation URI is included of each log entry.


Activity Host
-------------

An activity host manages the connection between MassTransit and the Activity. There are two hosts, one for execution and the other
for compensation. Only one host can exist on an endpoint, and the execute and compensate hosts should be on separate endpoints. Activities
are identified purely by URI, and therefore cannot share endpoints. *This means only one host per endpoint, no exceptions!*



