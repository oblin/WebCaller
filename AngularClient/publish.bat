:: Publish to linux server

:: replace index.html (need to set base href (need git bash))
:: cp src/index.html.linux src/index.html

:: 等待 ng build 之後再複製檔案到遠端
:: start /wait /b ng build --prod
:: call ng build --prod --base-href="/hopecareclient/"

:: Angular Version 17取消--prod  angular.json 可定義參數名稱
call ng build --configuration production --base-href="/angularclient/"
pscp -pw Cent@490910 -r dist/AngularClient/* centos@192.168.19.211:/var/www/angularclient