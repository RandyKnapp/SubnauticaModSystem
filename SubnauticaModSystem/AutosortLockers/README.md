# AutoSortLocker_Tweaks
Tweaks to the original ASL mod that I find useful. Only Below Zero at the moment.
 
To date, 19/06/2021, I have changed the following:
 
1) Made the background on the StandingLockers larger.
2) Changed some of the preprocessor directives #if/#endif.
  a) Added a directive to AutosortLockerSMLSaveData and AutosortLockerSMLBZSaveData.
  b) Added a directive to Subnautica_Data and SubnauticaZero_Data.
3) Left justified the list of Filters when there is more than one filter.
4) Truncated Category names when they exceed 17 or 22 characters. Small or Large locker
5) Elminated the AutoSorterCategory enum. It is now 100% read in from the new json files. 
6) All categories are now confifured in the categories.json file.
7) Hardcoded category names have been eliminated.
8) TechTypes are now read in from the techtypes.json.
9) Increased the size of the category/item picker from 7 to 10.
10) Left justified the filter names in the picker.
11) Changed the config to allow users to disable display of locker names. 
12) Changed the color picker.
13) Eliminated the coloring of the Locker Name.
14) Consolidated the color picker to one page and increased the size.
15) Changed colors.json to group colors together (whites, grays, reds, purples, etc.)
16) Changed LOCKER to "Color Settings"
 
Going forward, I would like to:
 
2) Truncate the TechType names that are read in from the game files.
