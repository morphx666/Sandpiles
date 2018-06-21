# Sandpiles
Simple Abelian sandpile model

Algorithm implemented from [Wikipedia's sandpile article](https://en.wikipedia.org/wiki/Abelian_sandpile_model) as seen on [The Coding Challenge #107: Sandpiles](https://youtu.be/diGjw5tghYU).

![Sandpiles](https://xfx.net/stackoverflow/sandpiles/sandpiles.png)

This implementation utilizes the [DirectBitmap](https://github.com/morphx666/Sandpiles/blob/master/Sandpiles/DirectBitmap.vb) class, which I introduced a [while back](https://github.com/morphx666/TupperFormula) and although it is extremely fast when you want to manipulate individual pixels and it’s 100% thread safe, it also lacks any type of aliasing support, so don't expect the results to look as nice as they do on the aforementioned articles/videos.
