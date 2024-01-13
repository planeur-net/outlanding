#!/bin/bash

#Remove Task line
sed '/-----Related Tasks-----/d' mountain_peaks_FR.cup > fr.cup
sed '/-----Related Tasks-----/d' mountain_peaks_CH.cup > ch.cup
sed '/-----Related Tasks-----/d' mountain_peaks_IT.cup > it.cup

# Remove first header line
sed -i '1d' ch.cup
sed -i '1d' it.cup

# Concatenate files
cat fr.cup <(echo) ch.cup <(echo) it.cup > mountain_peaks_FR_CH_IT.cup

# Clean up
rm fr.cup
rm ch.cup
rm it.cup