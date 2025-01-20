# RA-BCS

This is RA-BCS (Remote Api-Based Control Server).    

A "student project" aimed at using API's, listening to incoming messages and launch utilities.    
"Utilities" in this case - YT-DLP. You can find this project at: https://github.com/yt-dlp/yt-dlp    

## About

Objective:    
- [x] Launch YT-DLP at specified path with predefined options and URL that was provided throught API messages.    
Additional objectives:    
- [x] List files from specified folder, and move them to another folder.    

## Building

Tools:
1. Visual Studio 2022
2. .NET 9
3. Telegram.Bot library (22.2.0 and above)

Project is being developed using `Windows` operating system.    
Everything was tested when running `Windows 11 Pro`.    
Releases will be `Published build` for `Windows` operating systems (can change in the future). `Linux`, `MacOS` systems are not tested with releases.    

____

### Starting up

> [!NOTE]
> Directory containing `server.exe` must have `config.json` file.

If it doesn't exist - `config.json` will be created in the directory   

Path variables in `config.json` should have '\\' symbols escaped properly.    
Example:    
```
C":\\Users\\{USERNAME}\\RA-BCS\\server.exe
```    

> [!CAUTION]
> WARNING! DO NOT SHARE BOT TOKEN ANYWHERE!