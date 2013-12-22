Calculon 0.1
==========
An actor framework for .NET

Inspiration
===========
A lot of the principles of Calculon come from Akka, but since C# doesn't support pattern matching and has its own native asynchrony pattern, the TPL, Calculon eschews the single ``Receive`` method for method signatures as message contracts.

Philosophy
==========
The actor system, known as the ``Stage`` in calculon is a closed system. Actors are only accessible via messaging and always execute in a strictly single-threaded manner, i.e. no matter how many messages are sent to different methods, there will always be only one thread of execution within one actor.

Unfortunately, there is no way from preventing people to do "*bad*" things via compile time checking (such as manually spawning threads or sharing references), so Calculon tries to catch most such offenses as runtime errors, since the alternative of the execution model being violated is considered worse than the system failing at runtime.

Message Format
==============

What is an asynchronous method if not a contract of a message type accepted along with the response promised. Calculon uses the method signatures supported by async/await as its messaging:

* ``Task<T> MessageName<T>(arg1..argN)``
* ``Task MessageName(arg1..argN)``
* ``void MessageName(arg1..argN)``

Actors publish their messaging contracts as interfaces, but not have to implement the interface. I.e. Actors are true duck types, as long as a method that can handle the message contract exists, it can be used as a target for the contract. Right now, this is a 1-to-1 match between interface signatures and implementation signatures, but alternate receivers are planned, such as a general message receiver and callback style, such as ``void MessageName(arg1..argN,Action<T>)``.

Status
======
This is very early code. Actors can be created and called, but lifetime, supervisor hierarchies, async/await do not yet esists. See the project issues for features planned and not yet implemented. Helping hands are welcome.

Contributors
============
- Arne F. Claassen (sdether)
