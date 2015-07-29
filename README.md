# OneTox [![Build Status](https://jenkins.impy.me/buildStatus/icon?job=OneTox x86)](https://jenkins.impy.me/job/OneTox x86)
Work in progress Tox (https://tox.im/) client for the Windows Store.

The project was started within the confines of Google Summer of Code (https://www.google-melange.com/) in 2015. It's primal aim is to produce a stable and feature-rich Tox client that follows all Modern UI best practices and conforms to the feel and styling of Windows 8+ applications.

This client is under heavy development and it's very far from being complete. Pull or feature requests, constructive criticism or any other kind of contribution is very welcome!

## Features
* 1-to-1 messaging
* File trasnfers
* DNS discovery
* Faux offline messaging
* Profile import/export
* Save file encryption (currently disabled due to a bug)
* Multiprofile (in a very basic level)
* Typing notification
* Read receipts
* Message splitting
* Changing nospam
* Avatars
* File transfer resuming across core restarts

## TODO
* Audio
* Video
* Group chat
* Chat logs
* ...

## Screenshots
![Main Page](http://i.imgur.com/HrqjSxn.png)
![Chat Page](http://i.imgur.com/NHqHykf.png)

## Compiling
You'll need libtox.dll: https://jenkins.libtoxcore.so/view/Libs/job/toxcore_win32_dll/ (It's in the 'bin' folder of the zip.) You have to copy it to the 'libs' folder of OneTox.  
You'll also need to build the SharpToxPortable project (https://github.com/Impyy/SharpTox/blob/master/SharpTox/SharpTox%20Portable.csproj) with x86 as target and put the result (SharpTox Portable.dll) into 'libs' as well. After that you should be able to compile OneTox (x86).  
Of course we plan to add these dependencies to NuGet later.
