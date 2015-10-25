@echo off

echo Deleting old files
if exist ..\_localization rmdir /s /q _localization
mkdir ..\_localization


echo Copying content
copy Resources\en.txt               ..\_localization\en.txt
copy Resources\welcome_en.txt       ..\_localization\welcome.html
copy Statics.en\about.txt           ..\_localization\about.html
copy Statics.en\cookies.txt         ..\_localization\cookies.html

