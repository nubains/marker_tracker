# marker_tracker

Overview
---

This is an iPhone app that tracks a circular marker using the rear facing camera and OpenCV. The project would be broken down into two parts, the first being a C++ program that uses OpenCV to track the marker using my laptop webcam and the second being an iPhone application made with unity that does the same thing using the rear-facing camera on the phone.

The circular marker that will be tracked is shown below:

![Marker](https://octodex.github.com/images/marker.png)

Step 1: C++ Implementation
---

The C++ implementation was done in Xcode using OpenCV. There are many ways to approach the detection and tracking process. I first attmpted to use template tracking with an image of the marker as the template. I found this worked well for images but not for real time video tracking. As a result, I quickly moved on to Hough Circle detection. For this technique, I converted each frame of the video feed to the HSV colour space and set threshold values to isolate blue and red objects. I then applied the Hough Circle detection function on the HSV image and drew the circles that were found. With this method, the marker was being tracked fairly well, however, the radius of the enclosing circle fluctuated noticabley from frame to frame. This detracted from the smoothness of the tracking. To address this, I simply swapped out the Hough Circle detection function for the findContours function. I then identified the circular contours using minEnclosingCircle and drew all the circles with a radius larger than 40. This resulted in much smoother tracking.

The C++ OpenCV implementation can be found in marker_tracker/marker_tracker/main.cpp. Videos showing the results can be found in marker_tracker/Videos/laptop.

Step 2: Unity Implementation
---

Unity uses C# scripts and so, I would need to do some manupulation to run my OpenCV code. I used the OpenCVForUnity plugin due to the large amount of documentation available. Using this allowed me to access the OpenCV functions I needed in the Unity script. Due to some differences in variable types and input parameters to the functions, I had to remove some of the shape detection process. This led to a little less accuracy in circle detection, but by tuning the HSV threshold parameters again for my iPhone X camera, I was able to get reliable results in tracking the marker. THe circle detection is something that can be improved to make the tracking even better.

The Unity OpenCV implementation can be found in marker_tracker/CV_Eval/Assets/marker_tracker.cs. Videos showing the results can be found in marker_tracker/Videos/phone.

Usage: 
---

To use the Xcode project, download OpenCV C++ on your computer and set up the Xcode project accordingly. Then run the program using terminal by right-clicking the executable under Products in the Project navigator and selecting "Open with External Editor".

The Unity project can be opened with Unity, but the OpenCVForUnity plugin must be added to Assets for it to function.

The iPhone app can be loaded onto your iPhone by connecting your phone to your computer and opening Xcode. Then, in Xcode, select "Devices and Simulators" from the Window tab and drag and drop the .ipa file to the installed apps area under your phone. 
