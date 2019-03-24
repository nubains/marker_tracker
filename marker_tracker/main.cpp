#include <iostream>
#include <string>
#include <vector>
#include <deque>
#include "opencv2/opencv.hpp"
#include <opencv2/imgproc/imgproc.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/videoio/videoio.hpp>
#include <opencv2/objdetect/objdetect.hpp>
#include <opencv2/imgcodecs/imgcodecs.hpp>
#include "opencv2/core/core.hpp"

using namespace cv;
using namespace std;

//set default width and height for capture
const int FRAME_WIDTH = 640;
const int FRAME_HEIGHT = 480;

int main()
{
    //matrix to store each frame of the webcam feed
    Mat frame;
    //matrix to store hsv image
    Mat HSV;
    //intialize videocapture for webcam feed
    VideoCapture capture;
    //open webcam capture
    capture.open(0);
    //set height and width of capture window
    capture.set(CAP_PROP_FRAME_WIDTH,FRAME_WIDTH);
    capture.set(CAP_PROP_FRAME_HEIGHT,FRAME_HEIGHT);
    //start loop to process the video
    while(1){
        //read frame from webcam video feed
        capture.read(frame);
        
        //clone frame to new variable
        Mat cameraFeed = frame.clone();
        
        //apply blurring operations to frame
        GaussianBlur(cameraFeed, cameraFeed, Size(5,5), 0);
        medianBlur(cameraFeed, cameraFeed, 3);
        
        //convert to hsv colour space
        Mat hsv_image;
        cvtColor(cameraFeed, hsv_image, COLOR_BGR2HSV);
        
        //set threshholds for colour isolation
        Mat blue_hue_range;
        Mat red_hue_range;
        inRange(hsv_image, Scalar(85, 75, 75), Scalar(140, 255, 255), blue_hue_range);
        inRange(hsv_image, Scalar(90, 75, 75), Scalar(179, 255, 255), red_hue_range);
        
        //combine the above two ranges
        Mat hue_image;
        addWeighted(blue_hue_range, 1.0, red_hue_range, 1.0, 0.0, hue_image);
        
        //noise reduction
        GaussianBlur(hue_image, hue_image, Size(9, 9), 5);
        erode(hue_image, hue_image, 2);
        dilate(hue_image, hue_image, 2);
        
        //use findContours function to identify contours in image
        vector<vector<Point>> circles;
        vector<Vec4i> hierarchy;
        findContours(hue_image, circles, hierarchy, RETR_TREE, CHAIN_APPROX_SIMPLE, Point(0,0));
        
        vector<vector<Point> > contours_poly( circles.size() );
        vector<Rect> boundRect( circles.size() );
        vector<Point2f>center( circles.size() );
        vector<float>radius( circles.size() );
        
        //identify shape of contours and use minEnclosingCircle to create circles
        for( int i = 0; i < circles.size(); i++ )
        {
            approxPolyDP( Mat(circles[i]), contours_poly[i], 3, true );
            boundRect[i] = boundingRect( Mat(contours_poly[i]) );
            minEnclosingCircle( contours_poly[i], center[i], radius[i] );
        }

        //draw circles onto original image
        Mat drawing = Mat::zeros( hue_image.size(), CV_8UC3 );
        for( int i = 0; i< circles.size(); i++ )
        {
            //ensure radius of circle is > 20
            if(radius[i] > 20)
            {
                circle(frame, center[i], radius[i], Scalar(0, 255, 0), 2);
            }
        }
        
        
        //show frames
        imshow("hsv_feed", hue_image);
        imshow("marker_tracker", frame);
        
        //delay 20ms so that screen can refresh.
        waitKey(20);
    }
    return 0;
}
