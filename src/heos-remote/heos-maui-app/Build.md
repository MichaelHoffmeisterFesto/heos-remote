# Build APK

https://stackoverflow.com/questions/77283547/maui-app-how-to-install-on-android-phone-with-apk-file

In the folder that contains the .sln file:
```
dotnet publish -c Release -r android-arm64 -p:PackageFormat=Apk -f net9.0-android --sc true
```

