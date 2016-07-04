# Video-Controlled-Robot
Building a robot which could be controlled by video processing was the purpose of this project.

Robot was built using NXT. You can see nxt source code in NXT folder.

Video processing is done using opencv sharp.  You can download this library from [I'm an inline-style link](https://github.com/shimat/opencvsharp)

A white object is detected by it's brightness. If this object goes into a rectangle in left,right,up or downside of the frame, computer sends appropriate signals to the robot by bluetooth and the robot starts to move in the corresponding direction and keeps moving until the object is inside that rectangle.

 The C# code is as follows:
```cs
      CvPoint p = new CvPoint(0,0);
      CvPoint tmp;
      CvCapture capture;
      IplImage input;
      capture = Cv.CreateCameraCapture(0);                   //capture the main camera
      CvWindow w = new CvWindow("Camera");            //window to show the camera
      CvWindow w1 = new CvWindow("What Program sees");             //window to show the processed video
      Mat mtx = new Mat(capture.FrameWidth, capture.FrameHeight, MatrixType.U8C1);            //a one dimention matrix to store and process the captured video

      while (CvWindow.WaitKey(10) < 0)
      {
```
Here we just defined some attributes and we showing video and processing till user presses enter button
```cs
      input = Cv.QueryFrame(capture);     //get a frame
```
Get a frame and store it in input we will use it in rest of the code for processing and showing the results
```cs
      CvRect U = new CvRect(220, 10, 200, 100);           //Rectangle at top of the screen
      CvRect L = new CvRect(10, 140, 100, 200);           //Rectangle at left of the screen
      CvRect D = new CvRect(220, 370, 200, 100);         //Rectangle at down of the screen
      CvRect R = new CvRect(530, 140, 100, 200);         //Rectangle at top of the screen
     

     //draw Rectangles on the screen
     input.Rectangle(U, 0,2);
     input.Rectangle(L, 0, 2);
     input.Rectangle(D, 0, 2);
     input.Rectangle(R, 0, 2);
```
Create 4 rectangles and draw them on the screen
```cs
     CvCpp.ExtractImageCOI(input, mtx, 0);       //put taken frame to matrix
```
It converts the taken frame which is three dimentional to our one-dimentional matrix
```cs
      Cv.Threshold(mtx.ToCvMat(), mtx.ToCvMat(), 120, 255, 0);     //set numbers below 120 to 0 and above 120 until 255 to 255 (it removes other objects that arent bright from screen
      Cv.MinMaxLoc(mtx.ToCvMat(),out tmp,out p);        //get location of max number in mtx ( since it contains only 0 and 255) it will find first 255 in screen which is our object
```
Do some filtterings on matrix to find the bright object's location
```cs
      if ((p.X < 640) && (p.Y < 480))      //if p is in screen
      {
      //draw a rectangle on where the object is seen
      CvRect mask = new CvRect(p, new CvSize(10, 10));
      input.Rectangle(mask, 0,20);
      }
```
Draw rectangle where the object is seen
```cs
      if (isinrec(p, U)) send(3);             //if the object is in the top rectangle say robot to go up
      else if (isinrec(p, L)) send(2);      //if the object is in the left rectangle say robot to go left
      else if (isinrec(p, D)) send(4);     //if the object is in the bottom rectangle say robot to go down
      else if (isinrec(p, R)) send(1);    //if the object is in the right rectangle say robot to go right
```
If the object is in any of the rectangles send the command to robot to turn to that direction
```cs
      w.Image = input;          //show taken image on screen
      w1.Image = mtx.ToCvMat();         //show processed image on screen
```
Show camera output and processed image on screen.
