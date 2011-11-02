London Bike App
===============

**London Bike App** is an app I wrote in 2010, to allow people to find the nearest bike in the Barclays Cycle Hire scheme in London.

This app is (I hope) a good example of using MonoTouch, tho some of the code is a little messy (see the notes) by my current standards.

It makes a good use of the following iOS functions:

* Location awareness
* MonoTouch.Dialog for lists
* MKMapView and annotations, including drawing a route on the map
* Web services

The app is currently in the appstore: http://www.fastchicken.co.nz/londonbikeapp/ - however there is no point downloading it unless you live in London :)

License
=======

You can do whatever you like with this code (assume a MIT X11 license, same as MonoTouch.Dialog and Mono), however you cannot release it into the appstore as-is (or even with minor modifications), 
in an app for the London Cycle Scheme. Feel free to write an app for the Montreal scheme, or Paris, just not in competition
with London Bike App, in London, or use any part of the code in your non-london-bike-scheme apps.

Of course, I assume no liability for this code. Use it at your own risk.

Notes:
------
This code is up to 12 months old, so the best way to do things may have moved on since then. Join the MonoTouch forums (http://ios.xamarin.com/community) and ask if you have questions.

All the views were made with XCode 3, so may not work with XCode 4. But give it a try.

There is an overuse of delegate {} - I tend to do methods, not delegates, now. It does work, but it makes for a bit messy code.

Copyright and other notes
===================

**MonoTouch.Dialog**here is a very old, but customised version. If you are going to to an MT.D project,
get the real one from https://github.com/migueldeicaza/MonoTouch.Dialog

MT.D, btw, is totally essential if you are doing lists.

**Brilliant icons from Glyphish**http://glyphish.com

This package includes only the public (CC-BY) icons, which are available on the glyphish site. If you own the pro version,
drop the @2x version in next to them, and it'll work

If you don't have this icon set, and you are doing iOS development, you're a fool (or an icon designer)

**A few parts of the code are copyright their original authors**Reference to the source and the author is in the header.
