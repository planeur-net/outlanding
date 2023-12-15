# SeeYou CUPX file format
#### Copyright © 2019, Naviter d.o.o. All Rights Reserved
CUPX format is an extension of the cup format. It is actually a combination of two ZIP files combined in a
CUPX file.
It can be created by  
```
copy /b pics.zip + points.zip test.cupx 
```
or on linux  
```
cat pics.zip points.zip > test.cupx 
```
The first ZIP file (pics.zip) contains all the images in JPG format in the pics/ folder.  
The second ZIP file (points.zip) contains POINTS.CUP that cointains the waypoints with filenames of pictures
corresponding to points in the pics column of the CUP file (without folder name).