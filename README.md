# OneTox [![Build Status](https://jenkins.impy.me/buildStatus/icon?job=OneTox x86)](https://jenkins.impy.me/job/OneTox x86)
Work in progress Tox (https://tox.chat/) client targeting the Universal Windows Platform.

The project was started within the confines of [Google Summer of Code](https://www.google-melange.com/) in 2015. It's primal aim is to produce a stable and feature-rich Tox client that follows all Modern UI best practices and conforms to the feel and styling of Windows 10 Universal applications.

This client is under heavy development and it's very far from being complete. Pull or feature requests, constructive criticism or any other kind of contribution is very welcome!

## Important notes
* Currently the Jenkins build is failing until the new AV code is merged into toxcore. Please stay tuned!
* OneTox only runs on the desktop version of Windows 10 in it's current state. Windows Phone 10 support is planned for the future, but currently a full-featured desktop client is the main focus of the project.

## Features
* 1-to-1 messaging
* File trasnfers
* ~~DNS discovery~~ (currently not working due to obsolete API)
* Faux offline messaging
* Profile import/export
* ~~Save file encryption~~ (currently disabled due to a bug)
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
![Chat View](https://raw.githubusercontent.com/uruk/OneTox/master/screenshots/ChatView.png)
![ProfileSettings View](https://raw.githubusercontent.com/uruk/OneTox/master/screenshots/SettingsView.png)

## Compiling
You'll need ![libtox.dll](https://build.tox.chat/view/libtoxcore/job/libtoxcore_build_windows_x86_shared_release/) (It's in the 'bin' folder of the zip.) You have to copy it to the 'libs' folder of OneTox.  
Of course we plan to add this dependency to NuGet later.
