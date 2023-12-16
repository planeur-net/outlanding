#!/bin/bash
echo "Creating new .cupx file"
zip points.zip guide_aires_securite.cup
zip -r pics.zip Pics
cat pics.zip points.zip > guide_aires_securite.cupx