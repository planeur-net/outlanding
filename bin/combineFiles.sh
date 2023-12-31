#!/bin/bash
#Remove Task line
sed '/-----Related Tasks-----/d' guide_aires_securite.cup > part1.cup
sed '/-----Related Tasks-----/d' champs_des_alpes.cup > part2.cup
 
# Remove versionInfo
sed -i "$((grep -nm1 \"version= ./part1.cup || echo 1000000000:) | cut -f 1 -d:) d" ./part1.cup
sed -i "$((grep -nm1 \"version= ./part2.cup || echo 1000000000:) | cut -f 1 -d:) d" ./part2.cup

# Remove header
sed -i '1d' part2.cup

# Concatenate files
cat part1.cup part2.cup > combined_guide+champs.cup

# Add versionInfo
export v1=$(git log --pretty="%h %cI" -n1 -- guide_aires_securite.cup)
export v2=$(git log --pretty="%h %cI" -n1 -- champs_des_alpes.cup)
export versionInfo=`echo [guide_aires_securite]$v1 + [champs_des_alpes]$v2`
echo $versionInfo
sed -i "2i \"version=\",,,,,,,,,,,\"$versionInfo\",," ./combined_guide+champs.cup

# Cleanup
rm part1.cup part2.cup
 