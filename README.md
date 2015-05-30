# VRCAM

3D augmented reality using Google Cardboard

## About

VRCam uses a Google Cardboard and the phones front camera in order to implement a 3D augmented reality.  
The program lets the user see the real world in a Google Cardboard, but adds a 3D object to it.  
It uses a tracking algorithm in order to track a marker  (an object of a distinct colour) in real-time and project a 3D model onto it.

## Usage

### Basic

* Print out the marker PDF and place it somewhere
* Start the app
* Point your phone at the marker

### Custom models & options

Upon the first start, the program generates a config file. On Android phones it should be stored as `/sdcard/Android/data/com.schwalb.vrcam/files/config.json`.  
By editing this configuration file, a different 3D model may be chosen. The 3D model must be a Unity3D AssetBundle. Unity has a [tutorial](http://docs.unity3d.com/Manual/BuildingAssetBundles5x.html) on how to generate AssetBundles from 3D models.  
This file also allows modification of marker colour and other parameters.

## Build instructions

* Clone the repository and open the project in Unity 5.
* In the file menu, click *build settings*, choose *Android* and click "Build". An installable APK file will be generated

## Configuration options

The configuration file mainly has two parts. All parameters are mandatory. An example is given here:

```
{
  "analysis": {
    "markerColor": {
      "r": 235,
      "g": 125,
      "b": 35,
      "a": 1
    },
    "deviation": 80,
    "scaleFactor": 0.15,
    "binaryThreshold": 20,
    "edgeScale": 3,
    "markerMinWidth": 5,
    "markerMinHeight": 5
  },
  "model": {
    "assetURL": "file:///Users/caerostris/src/unity/VRCam/Assets/object.unity3d",
    "scaleX": 0.0015,
    "scaleY": 0.0015,
    "scaleZ": 0.0015,
    "distance": 3.0,
    "rotationX": 0.0,
    "rotationY": 0.0,
    "rotationZ": 0.0
  },
  "currentObject": null
}
```

### analysis

Options relevant for image analysis:

`analysis.markerColor.{ r,g,b, a }`: (0-255) RGB colour code of the marker. Leave a as 1.  
`analysis.deviation`: (0-255) Allowed diviation from the marker colour  
`analysis.scaleFactor`: (0-1.0) Scaling of the processed image. Smaller image = higher tracking rate, but lower tracking quality  
`analysis.binaryThreshold`: (0-255) Threshold for the binary filter. Every pixel with lower intensity than this value will not be considered when analysing the image  
`analysis.edgeScale`: (1-infinty) Higher value = better tracking but worse performance  
`analysis.markerMinWidth`: minimum width for an object to be considered as marker  
`analysis.markerMinHeight`: minimum height for an object to be considered as marker  

### model

Options relevant for the 3D model:

`model.assetURL`: (string) url of the `.unity3d` AssetBundle  
`model.scale{ X, Y, Z }`: Size scaling of the object  
`model.distance`: Distance of the object from the screen  
`model.rotation{ X, Y Z }`: rotation of the model

## How it works

### 3D space
VRCam uses Unity and the Google Cardboard SDK in order to create a 3D space. In front of the viewer is the 3D model, further in the background a "screen" showing the image of the phone's camera.
VRCam continously scans the current camera frame for the marker. If it was found, the objects position is set so that it appears at the marker's location.  
  
The glow effect in all following images is an effect in unity in not part of the actual pictures.

### Finding the marker

The tracking algorithm processes as many camera frames as possible. It starts with the raw camera image and applies various filters to find the marker.
![original image](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_orig.jpg)

#### Scaling

Image is scaled down to improve performance.
![scaled](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_scaled.jpg)

#### Euclidean Filter

A euclidean filter searches the picture for a specific colour and all colours *similar* within a given margin.  
Every pixel that does not match the given colour will be set to black. This filters removes everything from the image that does not match the marker's colour.
![euclidean filter](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_euclidean.jpg)

#### Grayscale filter

The image is converted to grayscale using an averaging algorithm.
![grayscale filter](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_grayscale.jpg)

#### Binary filter

The image is converted to a binary image. A binary image consists of only two colours, black and white. The intensity is measured for every pixel. Only if it is above a given threshold will be pixel be set to white. Thereby the image is filtered for noise.
![binary filter](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_binary.jpg)

#### Edge detection

An edge detection algorithm is applied. Everything except the every object's edges will be removed from the image.
![edge detection](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_edge.jpg)

#### Edge scaling

The edges are scaled up to make them more visible.
![edge scaling](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_scalededge.jpg)

#### Blob detection

This is where the actual tracking takes place: VRCam goes over the image pixel by pixel. When a white pixel is encountered, a breadth-first-search algorithm finds the coordinates of all white pixels that are connected to the first pixel. The outmost pixels to the left, right, top and bottom. Thereby, the position of all "blobs" in the image are detected. The biggest blob is assumed to be the marker.
![blob detection](https://0x.cx/ks225/vrcam/raw/master/bin/img/vrc_blob.jpg)

## License

All parts of the Google Cardboard SDK are released under the Apache License 2.0.  
All parts of the tracking code are released under the Mozilla Public License 2.0.

## Libraries

* [Google Cardboard SDK for Unity](https://developers.google.com/cardboard/unity/)
* [Newtonsoft Json libarary](http://www.newtonsoft.com/json)

