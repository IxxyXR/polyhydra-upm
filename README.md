# Polyhydra

![Screenshot](https://pro2-bar-s3-cdn-cf1.myportfolio.com/1e3b6316-da77-4fd2-a111-e12070c11b10/2977d391-d8a0-4759-8f3b-fe112b8957b8_rwc_0x22x975x549x975.png?h=f2ff1682c51247d1bc76e926872686e2)

Procedural generation of geometric forms in Unity.

<iframe width="560" height="315" src="https://www.youtube.com/embed/FPsXWHQO03Y" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture" allowfullscreen></iframe>

# Try it on the web

This is one possible UI that I started working on: http://www.polyhydra.org.uk/media/fastui/ Only a prototype but a quick way to get an intro into one possibly application of this toolkit. Keyboard controls listed onscreen Experimental and a bit buggy. Save often...

NOTE! - keyboard controls only in this particular web version. (It's keyboard only because it was a variation on a UI I created originally using MIDI devices which was a really nice tactile way to create - but difficult to share with people who didn't own the same MIDI controller as you - hence this version. There is a more conventional mouse+keyboard UI here: https://andybak.itch.io/polyhydra - but it's more clunky and less immediate )

# Overview

You start by choosing a uniform polyhedron: https://en.wikipedia.org/wiki/Uniform_polyhedron - these are generated using the Wythoff construction.

You can then stack up Conway Operators on top to create much more complex shapes: https://en.wikipedia.org/wiki/Conway_polyhedron_notation

# Documentation

See the [Wiki](https://github.com/IxxyXR/Polyhydra/wiki)

# Credits

As far as possible I'd like to licence this under the MIT licence or similar but the code has a complex heritage. 

Obviously the original work by Willem Wythoff and John Conway. And also countless other mathematicians who have formed a base for, contributed to and extended the work in this area. A special shout out to George Hart who is often co-credited with Conway due to the large amount of work he did exploring and extending Conway's original operators. 

The actual Wythoff code was based on https://github.com/kaonasi (which in turn is based on the work of Zvi Har’El: http://www.math.technion.ac.il/S/rl/kaleido/ ).

(Zvi Har'El has sadly passed away. I've tried to contact all potential copyright holders to see if it's OK to make use of their work as a basis for this but I've had no luck in getting a response. Please get in touch if you're an interested party.

Conway operator code was based on work by Will Pearson @mcneel which can be found here: https://github.com/pearswj/buckminster

Again - I tried to get in touch and didn't get a response. I'm not sure what the intended licence of that code is. It seems to be a standard copyright attribution but I wonder if this is an oversight more than the real intention of the author.

Multigrids is ported from https://github.com/kde/krita and is under the same licence (GPLv3)
Portions of grids.cs is from Antiprism and is MIT but should be attributed to Adrian Rossiter and Roger Kaufman: https://github.com/antiprism/antiprism/blob/master/COPYING

My original inspiration was 3DS Max's Hedra plugin which kept me entertained for quite a while nearly 2 decades ago. I think credit for that is due to Tom Hudson :-)

![Screenshot](https://github.com/Ixxy-Open-Source/wythoff-polyhedra/blob/master/0.png)

