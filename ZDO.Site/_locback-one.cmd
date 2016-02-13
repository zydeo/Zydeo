@echo off

if %1%==jian SET TLC=zh-CN
if %1%==fan SET TLC=zh-TW
if %1%==de SET TLC=de

copy ..\_localization\%TLC%\en.txt            Resources\%1%.txt
copy ..\_localization\%TLC%\welcome.html      Resources\welcome_%1%.txt
copy ..\_localization\%TLC%\noresults.html    Resources\noresults_%1%.txt

copy ..\_localization\%TLC%\about.html        Statics.%1%\about.txt
copy ..\_localization\%TLC%\options.html      Statics.%1%\options.txt
copy ..\_localization\%TLC%\cookies.html      Statics.%1%\cookies.txt

copy ..\_localization\%TLC%\ui.json           js-1.3\ui-%1%.js
