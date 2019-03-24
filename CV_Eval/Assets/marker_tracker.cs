using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;

namespace OpenCVForUnity
{
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class marker_tracker : MonoBehaviour
    {

        Texture2D texture;
        WebCamTextureToMatHelper webCamTextureToMatHelper;
        Mat grayMat;
        bool camMode = false;

        //Intialization function
        void Start ()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
            webCamTextureToMatHelper.Initialize ();
        }

        //Initialization function
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);

            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
           
                                    
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
                                    
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            grayMat = new Mat (webCamTextureMat.rows (), webCamTextureMat.cols (), CvType.CV_8UC1);
        }

        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");
            if (grayMat != null)
                grayMat.Dispose ();

            if (texture != null) {
                Texture2D.Destroy (texture);
                texture = null;
            }                        
        }

        public void OnWebCamTextureToMatHelperErrorOccurred (WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Image Processing function - called once per frame
        void Update ()
        {
            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                //load camera feed into matrix
                Mat frame = webCamTextureToMatHelper.GetMat ();

                //clone frame to new variable
                Mat cameraFeed = frame.clone();


                //apply blurring methods to image
                Imgproc.GaussianBlur(cameraFeed, cameraFeed, new Size(5, 5), 0);
                Imgproc.medianBlur(cameraFeed, cameraFeed, 3);


                //convert to hsv colour space
                Mat hsv_image = new Mat();
                Imgproc.cvtColor (cameraFeed, hsv_image, Imgproc.COLOR_BGR2HSV);

                //create thresholds for colour isolation
                Mat blue_hue_range = new Mat(); 
                Mat red_hue_range = new Mat();
                Mat lower_red = new Mat();
                Mat upper_red = new Mat();

                //upper and lower red colour thresholds 
                Core.inRange(hsv_image, new Scalar(0, 100, 100), new Scalar(10, 200, 200), lower_red);
                Core.inRange(hsv_image, new Scalar(160, 100, 100), new Scalar(179, 255, 255), upper_red);

                //add red thresholds together 
                Core.addWeighted(lower_red, 1.0, upper_red, 1.0, 0.0, red_hue_range);

                Core.inRange(hsv_image, new Scalar(115, 100, 100), new Scalar(135, 200, 200), blue_hue_range);

                //add red and blue thresholds together
                Mat hue_image = new Mat();
                Core.addWeighted(blue_hue_range, 1.0, red_hue_range, 1.0, 0.0, hue_image);

                //noise reduction on hsv image
                Imgproc.GaussianBlur(hue_image, hue_image, new Size(9, 9), 5);

                Mat erodeElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(3, 3));
                Mat dilateElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8));

                Imgproc.erode(hue_image, hue_image, erodeElement);
                Imgproc.dilate(hue_image, hue_image, dilateElement);

                //find contours in image
                System.Collections.Generic.List<MatOfPoint> circles = new System.Collections.Generic.List<MatOfPoint>();
                Mat hierarchy = new Mat();

                Imgproc.findContours(hue_image, circles, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE, new Point(0, 0));

                //find circles and draw if radius is > 30
                for (int i = 0; i < circles.Count; i++)
                {
                    Point pt = new Point();
                    float[] radius = new float[1];
                    Imgproc.minEnclosingCircle(new MatOfPoint2f(circles[i].toArray()), pt, radius);

                    int r = (int)radius[0];

                    if(r > 30)
                    {
                        Imgproc.circle(frame, pt, r, new Scalar(0, 255, 0), 3);
                    }
                }

                //output either frame with circles drawn or hsv feed depending on status of change camera button
                if(camMode == false)
                {
                    Utils.matToTexture2D(frame, texture, webCamTextureToMatHelper.GetBufferColors());
                }
                else
                {
                    Utils.matToTexture2D(hue_image, texture, webCamTextureToMatHelper.GetBufferColors());
                }
            }
        }

        void OnDestroy ()
        {
            webCamTextureToMatHelper.Dispose ();
        }

        //obsolete button functions 
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("OpenCVForUnityExample");
        }

        public void OnPlayButtonClick ()
        {
            webCamTextureToMatHelper.Play ();
        }

        public void OnPauseButtonClick ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        public void OnStopButtonClick ()
        {
            webCamTextureToMatHelper.Stop ();
        }


        //change feed between regular and hsv when button is pressed 
        public void OnChangeCameraButtonClick ()
        {
            if (camMode == false)
            {
                camMode = true;
            }
            else
            {
                camMode = false;
            }
        }
    }
}