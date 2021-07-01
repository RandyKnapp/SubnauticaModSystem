# AutoSortLocker_Tweaks
Tweaks to the original ASL mod that I find useful. Only Below Zero at the moment.
 
To date, 19/06/2021, I have changed the following:
 
1) Made the background on the StandingLockers larger.
2) Changed some of the preprocessor directives #if/#endif.
  a) Added a directive to AutosortLockerSMLSaveData and AutosortLockerSMLBZSaveData.
  b) Added a directive to Subnautica_Data and SubnauticaZero_Data.
3) Left justified the list of Filters when there is more than one filter.
4) Truncated Category names when they exceed 15 characters.
5) Sorted the AutoSorterCategory enum.
6) Consolidated and renamed some categories; there are now two upgrade categories, Base_UPG and Vehicle_UPG.
7) Renamed Alterra Stuff to Decorations.
8) Consolidated TechTypes and added some of the missing items.
9) Increased the size of the category/item picker from 7 to 10.
10) Left justified the filter names in the picker.
11) Replaced the existing Category and TechType system. Now the data is read 100% from the json files, the hard-coded fields were removed.
 
Going forward, I would like to:
 
1) Add an inventory count to the locker labels (i.e. [15] Gold, [30] Silver, etc.)
2) Truncate the TechType names that are read in from the game files.
3) Make these changes to SubNautica
4) Change the LOCKER label on the Color Settings screen
