# cs-imageTools

Currently contains:
1. Screencapture
2. accent colour finder

## Screen Capture
As the name suggests, it allows you to take screenshots.

using it is very simple:
```
ScreenCapture sc = new ScreenCapture();
Image img = sc.CaptureScreen();
```

## Accent Colour Finder
As the name suggests, this library allows you to find all accent colours of a given image

using it is very simple:
```
AccentColour.Finder finder = new AccentColour.Finder();
List<Color> colours = finder.analyse(img, 1).Result;
```
or
```
AccentColour.Finder finder = new AccentColour.Finder();
List<Color> colours = finder.analyse("myImage.png", 1).Result;
```
while the second argument represents the depth of the analysis process. Lower numbers mean that fewer pixels are skipped, leading to better results but longer processing time.
Note that very large images (> 2000 pixels in width) are automatically reduced to smaller image sizes (~ 1000 pixels in width). This means that an 8K image is treated like a full HD image. 8K is all well and good, but analysing the colour differences between the individual pixels is useless and also gives more results in the form of different shades of a colour.
